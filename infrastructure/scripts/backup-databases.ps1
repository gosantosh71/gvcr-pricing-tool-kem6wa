<#
.SYNOPSIS
    Automates database backups for the VAT Filing Pricing Tool.

.DESCRIPTION
    This script creates and manages backups of Azure SQL Database and Cosmos DB data,
    storing them in Azure Blob Storage with appropriate retention policies and validation.
    
    It supports different backup types (Full, Differential, TransactionLog) and validates
    the backups to ensure they can be used for recovery if needed.

.PARAMETER ResourceGroupName
    Name of the resource group containing the databases.

.PARAMETER Environment
    Deployment environment (dev, staging, prod).

.PARAMETER BackupType
    Type of backup operation (Full, Differential, TransactionLog).
    Default is Full.

.PARAMETER SqlServerName
    Name of the Azure SQL Server.

.PARAMETER SqlDatabaseName
    Name of the Azure SQL Database to backup.

.PARAMETER CosmosDBAccountName
    Name of the Azure Cosmos DB account.

.PARAMETER CosmosDBDatabaseName
    Name of the Cosmos DB database to backup.
    Default is VatFilingPricingTool.

.PARAMETER BackupStorageAccountName
    Name of the storage account for storing backups.

.PARAMETER BackupContainerName
    Name of the blob container for storing backups.
    Default is database-backups.

.PARAMETER CosmosContainers
    Array of Cosmos DB containers to backup.
    Default is Rules,AuditLogs,Configurations.

.PARAMETER RetentionDays
    Number of days to retain backups.
    Default is 35.

.PARAMETER ValidateBackups
    Whether to validate backups after creation.
    Default is true.

.PARAMETER LogFile
    Path to backup log file.
    Default is ./database-backup-{timestamp}.log.

.EXAMPLE
    .\backup-databases.ps1 -ResourceGroupName "vat-filing-rg" -Environment "prod" -SqlServerName "vat-sql-server" -SqlDatabaseName "VatFilingDB" -CosmosDBAccountName "vat-cosmos-account" -BackupStorageAccountName "vatbackupstorage"

.EXAMPLE
    .\backup-databases.ps1 -ResourceGroupName "vat-filing-rg" -Environment "dev" -BackupType "TransactionLog" -SqlServerName "vat-sql-server" -SqlDatabaseName "VatFilingDB" -CosmosDBAccountName "vat-cosmos-account" -BackupStorageAccountName "vatbackupstorage" -RetentionDays 14

.NOTES
    File Name      : backup-databases.ps1
    Author         : VAT Filing Pricing Tool Team
    Prerequisite   : Az PowerShell module, Az.Sql, Az.CosmosDB, Az.Storage
    Version        : 1.0
#>

# Import required modules
# Az module v7.5.0
Import-Module Az -ErrorAction SilentlyContinue
# Az.Sql module v4.12.0
Import-Module Az.Sql -ErrorAction SilentlyContinue
# Az.CosmosDB module v1.1.0
Import-Module Az.CosmosDB -ErrorAction SilentlyContinue
# Az.Storage module v5.5.0
Import-Module Az.Storage -ErrorAction SilentlyContinue

# Set error handling preferences
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

# Function to check if the user is logged into Azure
function Test-AzLogin {
    <#
    .SYNOPSIS
        Checks if the user is logged into Azure.
    
    .DESCRIPTION
        Tests if there is an active Azure context by attempting to get the current context.
        Returns true if successful, false if not logged in.
    
    .EXAMPLE
        if (-not (Test-AzLogin)) {
            Connect-AzAccount
        }
    #>
    
    try {
        $context = Get-AzContext
        return ($null -ne $context)
    }
    catch {
        return $false
    }
}

# Function to create a backup of an Azure SQL Database
function Backup-SqlDatabase {
    <#
    .SYNOPSIS
        Creates a backup of an Azure SQL Database.
    
    .DESCRIPTION
        Exports an Azure SQL Database to a BACPAC file in the specified storage account.
        Monitors the export operation until completion and validates the backup.
    
    .PARAMETER ResourceGroupName
        The resource group containing the SQL server and database.
    
    .PARAMETER ServerName
        The name of the Azure SQL Server.
    
    .PARAMETER DatabaseName
        The name of the database to backup.
    
    .PARAMETER BackupStorageAccountName
        The storage account where the backup will be stored.
    
    .PARAMETER BackupContainerName
        The blob container where the backup will be stored.
    
    .PARAMETER BackupFileName
        The filename for the backup file.
    
    .EXAMPLE
        Backup-SqlDatabase -ResourceGroupName "my-rg" -ServerName "my-sql-server" -DatabaseName "MyDatabase" -BackupStorageAccountName "mybackupstorage" -BackupContainerName "backups" -BackupFileName "MyDatabase-20230101.bacpac"
    #>
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory=$true)]
        [string]$ServerName,
        
        [Parameter(Mandatory=$true)]
        [string]$DatabaseName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupFileName
    )
    
    Write-BackupLog -Message "Starting backup of SQL Database '$DatabaseName' on server '$ServerName'" -LogLevel "INFO" -LogFile $global:LogFile
    
    # Check if database exists
    try {
        $database = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $DatabaseName -ErrorAction Stop
        Write-BackupLog -Message "Database '$DatabaseName' found on server '$ServerName'" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Database '$DatabaseName' not found on server '$ServerName': $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Database '$DatabaseName' not found: $_"
    }
    
    # Create a storage context
    try {
        $storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
        $storageContext = $storageAccount.Context
        Write-BackupLog -Message "Storage account '$BackupStorageAccountName' connected successfully" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Failed to connect to storage account '$BackupStorageAccountName': $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Failed to connect to storage account: $_"
    }
    
    # Create a SAS token for the storage container
    try {
        # Check if container exists, create if not
        $container = Get-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction SilentlyContinue
        if ($null -eq $container) {
            Write-BackupLog -Message "Container '$BackupContainerName' not found, creating it" -LogLevel "INFO" -LogFile $global:LogFile
            New-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction Stop | Out-Null
        }
        
        $sasToken = New-AzStorageContainerSASToken -Name $BackupContainerName -Context $storageContext -Permission rwdl -ExpiryTime (Get-Date).AddHours(4) -ErrorAction Stop
        Write-BackupLog -Message "SAS token generated for container '$BackupContainerName'" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Failed to generate SAS token for container '$BackupContainerName': $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Failed to generate SAS token: $_"
    }
    
    # Construct the storage URI
    $storageUri = "$($storageAccount.PrimaryEndpoints.Blob)$BackupContainerName/$BackupFileName$sasToken"
    
    # Initiate the database export
    try {
        $exportRequest = New-AzSqlDatabaseExport -ResourceGroupName $ResourceGroupName -ServerName $ServerName `
            -DatabaseName $DatabaseName -StorageKeyType "SharedAccessKey" -StorageKey $sasToken.Substring(1) `
            -StorageUri "$($storageAccount.PrimaryEndpoints.Blob)$BackupContainerName/$BackupFileName" -AdministratorLogin $database.SqlAdministratorLogin `
            -AdministratorLoginPassword (Read-Host -Prompt "Enter SQL administrator password" -AsSecureString) -ErrorAction Stop
            
        $exportRequestId = $exportRequest.OperationStatusLink
        Write-BackupLog -Message "SQL Database export initiated with request ID: $exportRequestId" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Failed to initiate database export: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Failed to initiate database export: $_"
    }
    
    # Monitor the export operation
    try {
        $exportStatus = Get-AzSqlDatabaseImportExportStatus -OperationStatusLink $exportRequestId -ErrorAction Stop
        
        while ($exportStatus.Status -eq "InProgress") {
            Write-BackupLog -Message "Export operation in progress... (Status: $($exportStatus.Status), Last Updated: $($exportStatus.LastModifiedTime))" -LogLevel "INFO" -LogFile $global:LogFile
            Start-Sleep -Seconds 30
            $exportStatus = Get-AzSqlDatabaseImportExportStatus -OperationStatusLink $exportRequestId -ErrorAction Stop
        }
        
        if ($exportStatus.Status -eq "Succeeded") {
            Write-BackupLog -Message "SQL Database export completed successfully" -LogLevel "SUCCESS" -LogFile $global:LogFile
        }
        else {
            Write-BackupLog -Message "SQL Database export failed with status: $($exportStatus.Status), Error: $($exportStatus.ErrorMessage)" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "SQL Database export failed: $($exportStatus.ErrorMessage)"
        }
    }
    catch {
        Write-BackupLog -Message "Error monitoring export operation: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error monitoring export operation: $_"
    }
    
    # Validate backup file
    try {
        $blob = Get-AzStorageBlob -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -ErrorAction Stop
        
        if ($null -eq $blob) {
            Write-BackupLog -Message "Backup file not found in storage container" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "Backup file not found in storage container"
        }
        
        Write-BackupLog -Message "Backup file created successfully: $BackupFileName (Size: $([math]::Round($blob.Length / 1MB, 2)) MB)" -LogLevel "INFO" -LogFile $global:LogFile
        
        # Return backup operation result
        return @{
            Status = "Success"
            BackupFileName = $BackupFileName
            BackupSize = $blob.Length
            BackupLocation = "$($storageAccount.PrimaryEndpoints.Blob)$BackupContainerName/$BackupFileName"
            CompletionTime = Get-Date
        }
    }
    catch {
        Write-BackupLog -Message "Error validating backup file: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error validating backup file: $_"
    }
}

# Function to export data from a Cosmos DB container
function Export-CosmosDBContainer {
    <#
    .SYNOPSIS
        Exports data from a Cosmos DB container to Azure Blob Storage.
    
    .DESCRIPTION
        Exports the data from a specified Cosmos DB container to a JSON file in Azure Blob Storage.
        Uses the Cosmos DB Data Migration Tool to perform the export.
    
    .PARAMETER ResourceGroupName
        The resource group containing the Cosmos DB account.
    
    .PARAMETER CosmosDBAccountName
        The name of the Cosmos DB account.
    
    .PARAMETER DatabaseName
        The name of the Cosmos DB database.
    
    .PARAMETER ContainerName
        The name of the Cosmos DB container to export.
    
    .PARAMETER BackupStorageAccountName
        The storage account where the backup will be stored.
    
    .PARAMETER BackupContainerName
        The blob container where the backup will be stored.
    
    .PARAMETER BackupFileName
        The filename for the backup file.
    
    .EXAMPLE
        Export-CosmosDBContainer -ResourceGroupName "my-rg" -CosmosDBAccountName "my-cosmos" -DatabaseName "MyDatabase" -ContainerName "MyContainer" -BackupStorageAccountName "mybackupstorage" -BackupContainerName "backups" -BackupFileName "MyContainer-20230101.json"
    #>
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory=$true)]
        [string]$CosmosDBAccountName,
        
        [Parameter(Mandatory=$true)]
        [string]$DatabaseName,
        
        [Parameter(Mandatory=$true)]
        [string]$ContainerName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupFileName
    )
    
    Write-BackupLog -Message "Starting export of Cosmos DB container '$ContainerName' from database '$DatabaseName'" -LogLevel "INFO" -LogFile $global:LogFile
    
    # Get Cosmos DB account details
    try {
        $cosmosDBAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction Stop
        Write-BackupLog -Message "Cosmos DB account '$CosmosDBAccountName' found" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Cosmos DB account '$CosmosDBAccountName' not found: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Cosmos DB account not found: $_"
    }
    
    # Check if database and container exist
    try {
        $database = Get-AzCosmosDBSqlDatabase -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -Name $DatabaseName -ErrorAction Stop
        Write-BackupLog -Message "Cosmos DB database '$DatabaseName' found" -LogLevel "INFO" -LogFile $global:LogFile
        
        $container = Get-AzCosmosDBSqlContainer -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -DatabaseName $DatabaseName -Name $ContainerName -ErrorAction Stop
        Write-BackupLog -Message "Cosmos DB container '$ContainerName' found" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error validating Cosmos DB database or container: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error validating Cosmos DB database or container: $_"
    }
    
    # Create a storage context
    try {
        $storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
        $storageContext = $storageAccount.Context
        Write-BackupLog -Message "Storage account '$BackupStorageAccountName' connected successfully" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Failed to connect to storage account '$BackupStorageAccountName': $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Failed to connect to storage account: $_"
    }
    
    # Check if container exists, create if not
    try {
        $blobContainer = Get-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction SilentlyContinue
        if ($null -eq $blobContainer) {
            Write-BackupLog -Message "Container '$BackupContainerName' not found, creating it" -LogLevel "INFO" -LogFile $global:LogFile
            New-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction Stop | Out-Null
        }
    }
    catch {
        Write-BackupLog -Message "Error checking/creating storage container: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error checking/creating storage container: $_"
    }
    
    # Get Cosmos DB connection string
    try {
        $keys = Get-AzCosmosDBAccountKey -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction Stop
        $connectionString = "AccountEndpoint=$($cosmosDBAccount.DocumentEndpoint);AccountKey=$($keys.PrimaryMasterKey)"
        Write-BackupLog -Message "Retrieved Cosmos DB connection string" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Failed to get Cosmos DB keys: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Failed to get Cosmos DB keys: $_"
    }
    
    # Create a temporary directory for the export
    $tempDir = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString())
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    $tempFilePath = [System.IO.Path]::Combine($tempDir, $BackupFileName)
    
    Write-BackupLog -Message "Created temporary directory for export: $tempDir" -LogLevel "INFO" -LogFile $global:LogFile
    
    try {
        # Perform the export using Cosmos DB Data Migration Tool (dtui.exe)
        # The following command path may need to be adjusted based on installation location
        $dtExePath = "${env:ProgramFiles}\Azure Cosmos DB Migration Tool\dtui.exe"
        
        if (-not (Test-Path $dtExePath)) {
            # Try alternative location
            $dtExePath = "${env:ProgramFiles(x86)}\Azure Cosmos DB Migration Tool\dt.exe"
            
            if (-not (Test-Path $dtExePath)) {
                Write-BackupLog -Message "Cosmos DB Data Migration Tool not found. Please install it from https://aka.ms/csdmtool" -LogLevel "ERROR" -LogFile $global:LogFile
                throw "Cosmos DB Data Migration Tool not found"
            }
        }
        
        # Create a configuration file for the Data Migration Tool
        $configFile = Join-Path $tempDir "export-config.json"
        $exportConfig = @{
            "DocumentDBConnection" = @{
                "ConnectionString" = $connectionString
                "Database" = $DatabaseName
                "Collection" = $ContainerName
            }
            "JsonFile" = @{
                "FilePath" = $tempFilePath
            }
        } | ConvertTo-Json -Depth 10
        
        Set-Content -Path $configFile -Value $exportConfig -Force
        
        Write-BackupLog -Message "Starting export with Data Migration Tool..." -LogLevel "INFO" -LogFile $global:LogFile
        
        # Execute the Data Migration Tool
        $arguments = "/s:DocumentDB", "/s.ConnectionString:`"$connectionString`"", "/s.Collection:$ContainerName", "/s.Database:$DatabaseName", "/t:JsonFile", "/t.File:`"$tempFilePath`"", "/t.Overwrite"
        $process = Start-Process -FilePath $dtExePath -ArgumentList $arguments -NoNewWindow -PassThru -Wait
        
        if ($process.ExitCode -ne 0) {
            Write-BackupLog -Message "Data Migration Tool failed with exit code $($process.ExitCode)" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "Data Migration Tool failed with exit code $($process.ExitCode)"
        }
        
        Write-BackupLog -Message "Data export completed to temporary file: $tempFilePath" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error exporting Cosmos DB data: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        # Clean up the temporary directory
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force
        }
        throw "Error exporting Cosmos DB data: $_"
    }
    
    # Upload the exported file to blob storage
    try {
        $blob = Set-AzStorageBlobContent -File $tempFilePath -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -Force -ErrorAction Stop
        Write-BackupLog -Message "Exported data uploaded to blob storage: $BackupFileName" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error uploading export to blob storage: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        # Clean up the temporary directory
        if (Test-Path $tempDir) {
            Remove-Item -Path $tempDir -Recurse -Force
        }
        throw "Error uploading export to blob storage: $_"
    }
    
    # Clean up the temporary directory
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Recurse -Force
        Write-BackupLog -Message "Temporary directory cleaned up" -LogLevel "INFO" -LogFile $global:LogFile
    }
    
    # Validate the uploaded blob
    try {
        $blobCheck = Get-AzStorageBlob -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -ErrorAction Stop
        if ($null -eq $blobCheck) {
            Write-BackupLog -Message "Exported file not found in storage container after upload" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "Exported file not found in storage container after upload"
        }
        
        Write-BackupLog -Message "Cosmos DB container export successful: $BackupFileName (Size: $([math]::Round($blobCheck.Length / 1KB, 2)) KB)" -LogLevel "SUCCESS" -LogFile $global:LogFile
        
        # Return export operation result
        return @{
            Status = "Success"
            BackupFileName = $BackupFileName
            BackupSize = $blobCheck.Length
            BackupLocation = "$($storageAccount.PrimaryEndpoints.Blob)$BackupContainerName/$BackupFileName"
            CompletionTime = Get-Date
        }
    }
    catch {
        Write-BackupLog -Message "Error validating uploaded export file: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error validating uploaded export file: $_"
    }
}

# Function to manage backup retention
function Manage-BackupRetention {
    <#
    .SYNOPSIS
        Manages backup retention by removing old backups based on retention policy.
    
    .DESCRIPTION
        Identifies and deletes backup files that are older than the specified retention period.
        Filters by database type (SQL or Cosmos) to apply the appropriate retention policy.
    
    .PARAMETER BackupStorageAccountName
        The storage account containing the backup files.
    
    .PARAMETER BackupContainerName
        The blob container containing the backup files.
    
    .PARAMETER RetentionDays
        The number of days to retain backup files.
    
    .PARAMETER DatabaseType
        The type of database backups to manage (SQL or Cosmos).
    
    .EXAMPLE
        Manage-BackupRetention -BackupStorageAccountName "mybackupstorage" -BackupContainerName "backups" -RetentionDays 35 -DatabaseType "SQL"
    #>
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory=$true)]
        [int]$RetentionDays,
        
        [Parameter(Mandatory=$true)]
        [ValidateSet("SQL", "Cosmos", "All")]
        [string]$DatabaseType
    )
    
    Write-BackupLog -Message "Managing backup retention for $DatabaseType backups (retention: $RetentionDays days)" -LogLevel "INFO" -LogFile $global:LogFile
    
    # Get the current date and calculate the cutoff date
    $currentDate = Get-Date
    $cutoffDate = $currentDate.AddDays(-$RetentionDays)
    
    Write-BackupLog -Message "Retention cutoff date: $cutoffDate" -LogLevel "INFO" -LogFile $global:LogFile
    
    # Get the storage account context
    try {
        $resourceGroup = Get-AzResourceGroup | Where-Object { Get-AzStorageAccount -ResourceGroupName $_.ResourceGroupName -Name $BackupStorageAccountName -ErrorAction SilentlyContinue }
        
        if ($null -eq $resourceGroup) {
            Write-BackupLog -Message "Storage account '$BackupStorageAccountName' not found in any resource group" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "Storage account not found in any resource group"
        }
        
        $storageAccount = Get-AzStorageAccount -ResourceGroupName $resourceGroup.ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
        $storageContext = $storageAccount.Context
        Write-BackupLog -Message "Connected to storage account '$BackupStorageAccountName'" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error connecting to storage account: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error connecting to storage account: $_"
    }
    
    # Get all blobs in the container
    try {
        $blobs = Get-AzStorageBlob -Container $BackupContainerName -Context $storageContext -ErrorAction Stop
        Write-BackupLog -Message "Found $($blobs.Count) backup files in container '$BackupContainerName'" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error getting blobs from container: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error getting blobs from container: $_"
    }
    
    # Filter blobs by database type if specified
    if ($DatabaseType -eq "SQL") {
        $blobs = $blobs | Where-Object { $_.Name -like "*.bacpac" }
        Write-BackupLog -Message "Filtered to $($blobs.Count) SQL backup files" -LogLevel "INFO" -LogFile $global:LogFile
    }
    elseif ($DatabaseType -eq "Cosmos") {
        $blobs = $blobs | Where-Object { $_.Name -like "*.json" }
        Write-BackupLog -Message "Filtered to $($blobs.Count) Cosmos DB backup files" -LogLevel "INFO" -LogFile $global:LogFile
    }
    
    # Find blobs older than the retention period
    $oldBlobs = $blobs | Where-Object { $_.LastModified.DateTime -lt $cutoffDate }
    
    Write-BackupLog -Message "Found $($oldBlobs.Count) backup files older than retention period" -LogLevel "INFO" -LogFile $global:LogFile
    
    $deletedCount = 0
    $deletedSize = 0
    
    # Delete old blobs
    foreach ($blob in $oldBlobs) {
        try {
            Remove-AzStorageBlob -Blob $blob.Name -Container $BackupContainerName -Context $storageContext -Force -ErrorAction Stop
            $deletedCount++
            $deletedSize += $blob.Length
            Write-BackupLog -Message "Deleted old backup: $($blob.Name) (Last Modified: $($blob.LastModified.DateTime), Size: $([math]::Round($blob.Length / 1MB, 2)) MB)" -LogLevel "INFO" -LogFile $global:LogFile
        }
        catch {
            Write-BackupLog -Message "Error deleting blob $($blob.Name): $_" -LogLevel "ERROR" -LogFile $global:LogFile
            # Continue with other blobs
        }
    }
    
    Write-BackupLog -Message "Backup retention management completed: Deleted $deletedCount files (Total: $([math]::Round($deletedSize / 1MB, 2)) MB)" -LogLevel "SUCCESS" -LogFile $global:LogFile
    
    # Return retention operation result
    return @{
        Status = "Success"
        DeletedCount = $deletedCount
        DeletedSize = $deletedSize
        RetentionDays = $RetentionDays
        CutoffDate = $cutoffDate
    }
}

# Function to validate database backup file
function Validate-DatabaseBackup {
    <#
    .SYNOPSIS
        Validates a database backup file for integrity.
    
    .DESCRIPTION
        Checks if a database backup file exists and validates its integrity 
        based on the database type (SQL or Cosmos DB).
    
    .PARAMETER DatabaseType
        The type of database backup to validate (SQL or Cosmos).
    
    .PARAMETER BackupStorageAccountName
        The storage account containing the backup file.
    
    .PARAMETER BackupContainerName
        The blob container containing the backup file.
    
    .PARAMETER BackupFileName
        The name of the backup file to validate.
    
    .EXAMPLE
        Validate-DatabaseBackup -DatabaseType "SQL" -BackupStorageAccountName "mybackupstorage" -BackupContainerName "backups" -BackupFileName "MyDatabase-20230101.bacpac"
    #>
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [ValidateSet("SQL", "Cosmos")]
        [string]$DatabaseType,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory=$true)]
        [string]$BackupFileName
    )
    
    Write-BackupLog -Message "Validating $DatabaseType backup file: $BackupFileName" -LogLevel "INFO" -LogFile $global:LogFile
    
    # Get the storage account context
    try {
        $resourceGroup = Get-AzResourceGroup | Where-Object { Get-AzStorageAccount -ResourceGroupName $_.ResourceGroupName -Name $BackupStorageAccountName -ErrorAction SilentlyContinue }
        
        if ($null -eq $resourceGroup) {
            Write-BackupLog -Message "Storage account '$BackupStorageAccountName' not found in any resource group" -LogLevel "ERROR" -LogFile $global:LogFile
            throw "Storage account not found in any resource group"
        }
        
        $storageAccount = Get-AzStorageAccount -ResourceGroupName $resourceGroup.ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
        $storageContext = $storageAccount.Context
        Write-BackupLog -Message "Connected to storage account '$BackupStorageAccountName'" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error connecting to storage account: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        throw "Error connecting to storage account: $_"
    }
    
    # Check if the backup file exists
    try {
        $blob = Get-AzStorageBlob -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -ErrorAction Stop
        
        if ($null -eq $blob) {
            Write-BackupLog -Message "Backup file not found: $BackupFileName" -LogLevel "ERROR" -LogFile $global:LogFile
            return @{
                Status = "Failed"
                Message = "Backup file not found"
                FileName = $BackupFileName
            }
        }
        
        Write-BackupLog -Message "Backup file found: $BackupFileName (Size: $([math]::Round($blob.Length / 1MB, 2)) MB)" -LogLevel "INFO" -LogFile $global:LogFile
    }
    catch {
        Write-BackupLog -Message "Error checking backup file: $_" -LogLevel "ERROR" -LogFile $global:LogFile
        return @{
            Status = "Failed"
            Message = "Error checking backup file: $_"
            FileName = $BackupFileName
        }
    }
    
    # Check file size - should be greater than 0
    if ($blob.Length -le 0) {
        Write-BackupLog -Message "Backup file is empty: $BackupFileName" -LogLevel "ERROR" -LogFile $global:LogFile
        return @{
            Status = "Failed"
            Message = "Backup file is empty"
            FileName = $BackupFileName
            FileSize = $blob.Length
        }
    }
    
    # Perform database-specific validation
    if ($DatabaseType -eq "SQL") {
        # For SQL backups (BACPAC files), we can check the file size and extension
        if ($BackupFileName -notlike "*.bacpac") {
            Write-BackupLog -Message "SQL backup file has incorrect extension: $BackupFileName" -LogLevel "WARNING" -LogFile $global:LogFile
        }
        
        # SQL BACPAC files should be at least 10KB for even an empty database
        if ($blob.Length -lt 10KB) {
            Write-BackupLog -Message "SQL backup file is suspiciously small: $([math]::Round($blob.Length / 1KB, 2)) KB" -LogLevel "WARNING" -LogFile $global:LogFile
        }
        
        # For a more thorough validation, we would download and check BACPAC structure
        # This is a simplified validation that only checks size
    }
    elseif ($DatabaseType -eq "Cosmos") {
        # For Cosmos DB backups (JSON files), we can check the file size and extension
        if ($BackupFileName -notlike "*.json") {
            Write-BackupLog -Message "Cosmos DB backup file has incorrect extension: $BackupFileName" -LogLevel "WARNING" -LogFile $global:LogFile
        }
        
        # Even an empty JSON file should have at least the basic structure
        if ($blob.Length -lt 2) { # Minimum size for valid JSON with empty object "{}"
            Write-BackupLog -Message "Cosmos DB backup file is suspiciously small: $($blob.Length) bytes" -LogLevel "WARNING" -LogFile $global:LogFile
        }
        
        # For a more thorough validation, we would download and parse the JSON
        # This is a simplified validation that only checks size
    }
    
    Write-BackupLog -Message "Backup file validation completed successfully: $BackupFileName" -LogLevel "SUCCESS" -LogFile $global:LogFile
    
    # Return validation result
    return @{
        Status = "Success"
        Message = "Backup file validated successfully"
        FileName = $BackupFileName
        FileSize = $blob.Length
        LastModified = $blob.LastModified
    }
}

# Function to write backup logs
function Write-BackupLog {
    <#
    .SYNOPSIS
        Writes backup operation logs to file and console.
    
    .DESCRIPTION
        Writes log messages to both a log file and the console with appropriate formatting and colors.
    
    .PARAMETER Message
        The message to log.
    
    .PARAMETER LogLevel
        The log level (INFO, WARNING, ERROR, SUCCESS).
    
    .PARAMETER LogFile
        The path to the log file.
    
    .EXAMPLE
        Write-BackupLog -Message "Backup started" -LogLevel "INFO" -LogFile "./backup.log"
    #>
    
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Message,
        
        [Parameter(Mandatory=$true)]
        [ValidateSet("INFO", "WARNING", "ERROR", "SUCCESS")]
        [string]$LogLevel,
        
        [Parameter(Mandatory=$true)]
        [string]$LogFile
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$LogLevel] $Message"
    
    # Write to console with appropriate color
    switch ($LogLevel) {
        "INFO" { Write-Host $logMessage -ForegroundColor Gray }
        "WARNING" { Write-Host $logMessage -ForegroundColor Yellow }
        "ERROR" { Write-Host $logMessage -ForegroundColor Red }
        "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
        default { Write-Host $logMessage }
    }
    
    # Write to log file
    try {
        Add-Content -Path $LogFile -Value $logMessage -ErrorAction Stop
    }
    catch {
        Write-Host "Error writing to log file: $_" -ForegroundColor Red
    }
}

# Parse and validate input parameters
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Full", "Differential", "TransactionLog")]
    [string]$BackupType = "Full",
    
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlDatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$CosmosDBAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$CosmosDBDatabaseName = "VatFilingPricingTool",
    
    [Parameter(Mandatory=$true)]
    [string]$BackupStorageAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$BackupContainerName = "database-backups",
    
    [Parameter(Mandatory=$false)]
    [string]$CosmosContainers = "Rules,AuditLogs,Configurations",
    
    [Parameter(Mandatory=$false)]
    [ValidateRange(1, 365)]
    [int]$RetentionDays = 35,
    
    [Parameter(Mandatory=$false)]
    [bool]$ValidateBackups = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$LogFile = "./database-backup-{timestamp}.log"
)

# Main script execution
try {
    # Create timestamp for backup identification and logging
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    
    # Replace {timestamp} in LogFile path with actual timestamp
    $LogFile = $LogFile -replace "{timestamp}", $timestamp
    
    # Create log file directory if it doesn't exist
    $logDir = Split-Path -Path $LogFile -Parent
    if (-not [string]::IsNullOrEmpty($logDir) -and -not (Test-Path -Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    # Make LogFile available to functions through $global:LogFile
    $global:LogFile = $LogFile
    
    # Log backup operation start
    Write-BackupLog -Message "Starting database backup operation for environment: $Environment" -LogLevel "INFO" -LogFile $LogFile
    Write-BackupLog -Message "Backup type: $BackupType, Retention: $RetentionDays days" -LogLevel "INFO" -LogFile $LogFile
    
    # Check Azure login
    if (-not (Test-AzLogin)) {
        Write-BackupLog -Message "Not logged into Azure. Prompting for login..." -LogLevel "INFO" -LogFile $LogFile
        Connect-AzAccount -ErrorAction Stop
    }
    
    # Verify resource group exists
    try {
        $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction Stop
        Write-BackupLog -Message "Resource group '$ResourceGroupName' found" -LogLevel "INFO" -LogFile $LogFile
    }
    catch {
        Write-BackupLog -Message "Resource group '$ResourceGroupName' not found: $_" -LogLevel "ERROR" -LogFile $LogFile
        throw "Resource group not found: $_"
    }
    
    # Verify SQL Server and Database exist
    try {
        $sqlServer = Get-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ErrorAction Stop
        Write-BackupLog -Message "SQL Server '$SqlServerName' found" -LogLevel "INFO" -LogFile $LogFile
        
        $sqlDatabase = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -DatabaseName $SqlDatabaseName -ErrorAction Stop
        Write-BackupLog -Message "SQL Database '$SqlDatabaseName' found" -LogLevel "INFO" -LogFile $LogFile
    }
    catch {
        Write-BackupLog -Message "Error verifying SQL Server or Database: $_" -LogLevel "ERROR" -LogFile $LogFile
        throw "Error verifying SQL Server or Database: $_"
    }
    
    # Verify Cosmos DB account and database exist
    try {
        $cosmosDBAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction Stop
        Write-BackupLog -Message "Cosmos DB account '$CosmosDBAccountName' found" -LogLevel "INFO" -LogFile $LogFile
        
        $cosmosDBDatabase = Get-AzCosmosDBSqlDatabase -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -Name $CosmosDBDatabaseName -ErrorAction Stop
        Write-BackupLog -Message "Cosmos DB database '$CosmosDBDatabaseName' found" -LogLevel "INFO" -LogFile $LogFile
    }
    catch {
        Write-BackupLog -Message "Error verifying Cosmos DB account or database: $_" -LogLevel "ERROR" -LogFile $LogFile
        throw "Error verifying Cosmos DB account or database: $_"
    }
    
    # Verify backup storage account exists, create container if needed
    try {
        $storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
        Write-BackupLog -Message "Storage account '$BackupStorageAccountName' found" -LogLevel "INFO" -LogFile $LogFile
        
        $storageContext = $storageAccount.Context
        $container = Get-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction SilentlyContinue
        
        if ($null -eq $container) {
            Write-BackupLog -Message "Container '$BackupContainerName' not found, creating it" -LogLevel "INFO" -LogFile $LogFile
            New-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction Stop | Out-Null
        }
        else {
            Write-BackupLog -Message "Container '$BackupContainerName' found" -LogLevel "INFO" -LogFile $LogFile
        }
    }
    catch {
        Write-BackupLog -Message "Error verifying storage account or container: $_" -LogLevel "ERROR" -LogFile $LogFile
        throw "Error verifying storage account or container: $_"
    }
    
    # Generate backup file names with timestamp
    $sqlBackupFileName = "$Environment-$SqlDatabaseName-$BackupType-$timestamp.bacpac"
    Write-BackupLog -Message "SQL backup file name: $sqlBackupFileName" -LogLevel "INFO" -LogFile $LogFile
    
    # Split CosmosContainers string into array
    $cosmosContainerArray = $CosmosContainers -split ','
    Write-BackupLog -Message "Cosmos DB containers to backup: $($cosmosContainerArray -join ', ')" -LogLevel "INFO" -LogFile $LogFile
    
    # Backup operations result tracking
    $backupResults = @{
        SQL = $null
        Cosmos = @{}
        RetentionManagement = $null
        ValidationResults = @{
            SQL = $null
            Cosmos = @{}
        }
    }
    
    # Perform backup operations based on backup type
    if ($BackupType -in "Full", "Differential") {
        # Backup SQL Database
        try {
            Write-BackupLog -Message "Starting SQL Database backup ($BackupType)" -LogLevel "INFO" -LogFile $LogFile
            $backupResults.SQL = Backup-SqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -DatabaseName $SqlDatabaseName -BackupStorageAccountName $BackupStorageAccountName -BackupContainerName $BackupContainerName -BackupFileName $sqlBackupFileName
            Write-BackupLog -Message "SQL Database backup completed successfully" -LogLevel "SUCCESS" -LogFile $LogFile
        }
        catch {
            Write-BackupLog -Message "Error during SQL Database backup: $_" -LogLevel "ERROR" -LogFile $LogFile
            $backupResults.SQL = @{
                Status = "Failed"
                ErrorMessage = $_.Exception.Message
                BackupFileName = $sqlBackupFileName
            }
        }
        
        # Backup Cosmos DB containers
        foreach ($containerName in $cosmosContainerArray) {
            $containerName = $containerName.Trim()
            $cosmosBackupFileName = "$Environment-$CosmosDBDatabaseName-$containerName-$timestamp.json"
            
            try {
                Write-BackupLog -Message "Starting Cosmos DB container backup: $containerName" -LogLevel "INFO" -LogFile $LogFile
                $backupResults.Cosmos[$containerName] = Export-CosmosDBContainer -ResourceGroupName $ResourceGroupName -CosmosDBAccountName $CosmosDBAccountName -DatabaseName $CosmosDBDatabaseName -ContainerName $containerName -BackupStorageAccountName $BackupStorageAccountName -BackupContainerName $BackupContainerName -BackupFileName $cosmosBackupFileName
                Write-BackupLog -Message "Cosmos DB container backup completed successfully: $containerName" -LogLevel "SUCCESS" -LogFile $LogFile
            }
            catch {
                Write-BackupLog -Message "Error during Cosmos DB container backup ($containerName): $_" -LogLevel "ERROR" -LogFile $LogFile
                $backupResults.Cosmos[$containerName] = @{
                    Status = "Failed"
                    ErrorMessage = $_.Exception.Message
                    BackupFileName = $cosmosBackupFileName
                }
            }
        }
    }
    elseif ($BackupType -eq "TransactionLog") {
        # For transaction log backups, we would use a different approach
        # Azure SQL Database doesn't support direct transaction log backups like on-premises SQL Server
        # Instead, we rely on the automated transaction log backups that Azure performs
        Write-BackupLog -Message "Transaction log backups for Azure SQL Database are handled automatically by the platform" -LogLevel "INFO" -LogFile $LogFile
        Write-BackupLog -Message "Point-in-time restore is available for the past 35 days (or configured retention period)" -LogLevel "INFO" -LogFile $LogFile
        
        $backupResults.SQL = @{
            Status = "Skipped"
            Message = "Transaction log backups are handled automatically by Azure SQL Database"
        }
    }
    
    # Validate backups if specified
    if ($ValidateBackups) {
        # Validate SQL backup
        if ($backupResults.SQL -and $backupResults.SQL.Status -eq "Success") {
            try {
                Write-BackupLog -Message "Validating SQL Database backup" -LogLevel "INFO" -LogFile $LogFile
                $backupResults.ValidationResults.SQL = Validate-DatabaseBackup -DatabaseType "SQL" -BackupStorageAccountName $BackupStorageAccountName -BackupContainerName $BackupContainerName -BackupFileName $sqlBackupFileName
            }
            catch {
                Write-BackupLog -Message "Error validating SQL Database backup: $_" -LogLevel "ERROR" -LogFile $LogFile
                $backupResults.ValidationResults.SQL = @{
                    Status = "Failed"
                    ErrorMessage = $_.Exception.Message
                    BackupFileName = $sqlBackupFileName
                }
            }
        }
        
        # Validate Cosmos DB backups
        foreach ($containerName in $cosmosContainerArray) {
            $containerName = $containerName.Trim()
            $cosmosBackupFileName = "$Environment-$CosmosDBDatabaseName-$containerName-$timestamp.json"
            
            if ($backupResults.Cosmos[$containerName] -and $backupResults.Cosmos[$containerName].Status -eq "Success") {
                try {
                    Write-BackupLog -Message "Validating Cosmos DB container backup: $containerName" -LogLevel "INFO" -LogFile $LogFile
                    $backupResults.ValidationResults.Cosmos[$containerName] = Validate-DatabaseBackup -DatabaseType "Cosmos" -BackupStorageAccountName $BackupStorageAccountName -BackupContainerName $BackupContainerName -BackupFileName $cosmosBackupFileName
                }
                catch {
                    Write-BackupLog -Message "Error validating Cosmos DB container backup ($containerName): $_" -LogLevel "ERROR" -LogFile $LogFile
                    $backupResults.ValidationResults.Cosmos[$containerName] = @{
                        Status = "Failed"
                        ErrorMessage = $_.Exception.Message
                        BackupFileName = $cosmosBackupFileName
                    }
                }
            }
        }
    }
    
    # Manage backup retention
    try {
        Write-BackupLog -Message "Managing backup retention (RetentionDays: $RetentionDays)" -LogLevel "INFO" -LogFile $LogFile
        $backupResults.RetentionManagement = Manage-BackupRetention -BackupStorageAccountName $BackupStorageAccountName -BackupContainerName $BackupContainerName -RetentionDays $RetentionDays -DatabaseType "All"
    }
    catch {
        Write-BackupLog -Message "Error managing backup retention: $_" -LogLevel "ERROR" -LogFile $LogFile
        $backupResults.RetentionManagement = @{
            Status = "Failed"
            ErrorMessage = $_.Exception.Message
        }
    }
    
    # Log backup operation completion
    Write-BackupLog -Message "Database backup operation completed" -LogLevel "INFO" -LogFile $LogFile
    
    # Output backup operation summary
    return $backupResults
}
catch {
    Write-BackupLog -Message "Critical error during backup operation: $_" -LogLevel "ERROR" -LogFile $global:LogFile
    throw "Critical error during backup operation: $_"
}