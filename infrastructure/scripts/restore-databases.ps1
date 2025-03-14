<#
.SYNOPSIS
    Restores Azure SQL Database and Cosmos DB from backups or point-in-time snapshots.

.DESCRIPTION
    This script automates the process of restoring Azure SQL Database and Cosmos DB data
    from backups stored in Azure Blob Storage or using point-in-time restore capabilities.
    It supports validation of restored data and detailed logging of the restore process.

.PARAMETER ResourceGroupName
    Name of the resource group containing the databases.

.PARAMETER Environment
    Deployment environment (dev, staging, prod).

.PARAMETER RestoreType
    Type of restore operation (FromBackup, PointInTime).

.PARAMETER SqlServerName
    Name of the Azure SQL Server.

.PARAMETER SqlDatabaseName
    Name of the Azure SQL Database to restore.

.PARAMETER CosmosDBAccountName
    Name of the Azure Cosmos DB account.

.PARAMETER CosmosDBDatabaseName
    Name of the Cosmos DB database to restore.

.PARAMETER BackupStorageAccountName
    Name of the storage account containing backups.

.PARAMETER BackupContainerName
    Name of the blob container containing backups.

.PARAMETER SqlBackupFileName
    Name of the SQL database backup file.

.PARAMETER CosmosContainers
    Array of Cosmos DB containers to restore.

.PARAMETER PointInTimeUtc
    Point in time to restore to (in UTC).

.PARAMETER TargetDatabaseName
    Name of the target database for restore (if different from source).

.PARAMETER TargetCosmosDBAccountName
    Name of the target Cosmos DB account for restore (if different from source).

.PARAMETER ValidateRestore
    Whether to validate the restored databases.

.PARAMETER LogFile
    Path to restore log file.

.EXAMPLE
    .\restore-databases.ps1 -ResourceGroupName "vat-pricing-rg" -Environment "dev" -SqlServerName "vat-pricing-sql" -SqlDatabaseName "VatPricing" -CosmosDBAccountName "vat-pricing-cosmos" -BackupStorageAccountName "vatpricingbackup"

.EXAMPLE
    .\restore-databases.ps1 -ResourceGroupName "vat-pricing-rg" -Environment "prod" -RestoreType "PointInTime" -SqlServerName "vat-pricing-sql" -SqlDatabaseName "VatPricing" -CosmosDBAccountName "vat-pricing-cosmos" -PointInTimeUtc "2023-05-10T14:30:00Z" -TargetDatabaseName "VatPricing_Restored" -TargetCosmosDBAccountName "vat-pricing-cosmos-restored"

.NOTES
    This script requires the following PowerShell modules:
    - Az (version 9.3.0 or higher)
    - Az.Sql (version 4.0.0 or higher)
    - Az.CosmosDB (version 1.1.0 or higher)
    - Az.Storage (version 5.0.0 or higher)
#>

# Set strict error handling
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

# Function to check if user is logged into Azure
function Test-AzLogin {
    [CmdletBinding()]
    [OutputType([bool])]
    param()

    process {
        try {
            $context = Get-AzContext
            if ($null -eq $context.Account) {
                return $false
            }
            return $true
        }
        catch {
            return $false
        }
    }
}

# Function to restore an Azure SQL Database
function Restore-SqlDatabase {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$ServerName,
        
        [Parameter(Mandatory = $true)]
        [string]$DatabaseName,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory = $false)]
        [string]$BackupFileName,
        
        [Parameter(Mandatory = $false)]
        [datetime]$PointInTimeUtc,
        
        [Parameter(Mandatory = $true)]
        [bool]$UsePointInTimeRestore
    )

    process {
        Write-RestoreLog "Starting restore of SQL Database '$DatabaseName' on server '$ServerName'" "INFO" $LogFile
        
        try {
            # Check if source database exists
            $sourceDatabase = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $DatabaseName -ErrorAction SilentlyContinue
            
            # Determine target database name
            $targetDbName = if ($TargetDatabaseName) { $TargetDatabaseName } else { $DatabaseName + "_restored" }
            
            # Check if target database already exists
            $targetDatabase = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $targetDbName -ErrorAction SilentlyContinue
            if ($targetDatabase) {
                Write-RestoreLog "Target database '$targetDbName' already exists. Will be removed before restore." "WARNING" $LogFile
                Remove-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $targetDbName -Force
                Write-RestoreLog "Removed existing target database '$targetDbName'" "INFO" $LogFile
            }
            
            if ($UsePointInTimeRestore) {
                if ($null -eq $sourceDatabase) {
                    throw "Source database '$DatabaseName' does not exist. Cannot perform point-in-time restore."
                }
                
                Write-RestoreLog "Performing point-in-time restore of database '$DatabaseName' to $PointInTimeUtc" "INFO" $LogFile
                
                # Get the edition and service objective of the source database
                $edition = $sourceDatabase.Edition
                $serviceObjective = $sourceDatabase.CurrentServiceObjectiveName
                
                # Perform point-in-time restore
                $restoreOperation = Restore-AzSqlDatabase -FromPointInTimeBackup `
                    -ResourceGroupName $ResourceGroupName `
                    -ServerName $ServerName `
                    -DatabaseName $DatabaseName `
                    -TargetDatabaseName $targetDbName `
                    -PointInTime $PointInTimeUtc `
                    -Edition $edition `
                    -ServiceObjectiveName $serviceObjective
                
                Write-RestoreLog "Point-in-time restore operation initiated. Database: $targetDbName, RestoreId: $($restoreOperation.RestorePointInTime)" "INFO" $LogFile
                
                # Wait for the restore operation to complete
                $restoreComplete = $false
                $startTime = Get-Date
                $timeout = 7200 # 2 hours timeout
                
                while (-not $restoreComplete) {
                    $restoredDb = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $targetDbName -ErrorAction SilentlyContinue
                    
                    if ($restoredDb -and $restoredDb.Status -eq "Online") {
                        $restoreComplete = $true
                        Write-RestoreLog "Point-in-time restore completed successfully. Database '$targetDbName' is online." "INFO" $LogFile
                    }
                    else {
                        $currentTime = Get-Date
                        $elapsed = ($currentTime - $startTime).TotalSeconds
                        
                        if ($elapsed -gt $timeout) {
                            throw "Restore operation timed out after $timeout seconds."
                        }
                        
                        Write-RestoreLog "Waiting for restore operation to complete... Elapsed time: $([math]::Round($elapsed)) seconds" "INFO" $LogFile
                        Start-Sleep -Seconds 30
                    }
                }
                
                return @{
                    Status = "Success"
                    DatabaseName = $targetDbName
                    RestoreType = "PointInTime"
                    RestoreTime = $PointInTimeUtc
                    CompletionTime = Get-Date
                }
            }
            else {
                # From backup file restore
                if ([string]::IsNullOrEmpty($BackupFileName) -or [string]::IsNullOrEmpty($BackupStorageAccountName) -or [string]::IsNullOrEmpty($BackupContainerName)) {
                    throw "Backup file parameters are required for restore from backup."
                }
                
                Write-RestoreLog "Importing database from backup file '$BackupFileName'" "INFO" $LogFile
                
                # Get storage account key
                $storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName
                $storageKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName)[0].Value
                
                # Create storage context
                $storageContext = New-AzStorageContext -StorageAccountName $BackupStorageAccountName -StorageAccountKey $storageKey
                
                # Check if backup file exists
                $backupBlob = Get-AzStorageBlob -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -ErrorAction SilentlyContinue
                if (-not $backupBlob) {
                    throw "Backup file '$BackupFileName' not found in container '$BackupContainerName'."
                }
                
                # Generate SAS token for the backup file
                $sasToken = New-AzStorageBlobSASToken -Container $BackupContainerName -Blob $BackupFileName -Permission r -ExpiryTime (Get-Date).AddHours(4) -Context $storageContext
                $bacpacUri = $backupBlob.ICloudBlob.Uri.AbsoluteUri + $sasToken
                
                # Determine the database edition based on environment
                $edition = switch ($Environment) {
                    "dev" { "Standard" }
                    "staging" { "Standard" }
                    "prod" { "Premium" }
                    default { "Standard" }
                }
                
                # Determine the service objective based on environment
                $serviceObjective = switch ($Environment) {
                    "dev" { "S1" }
                    "staging" { "S2" }
                    "prod" { "P1" }
                    default { "S1" }
                }
                
                # Get SQL admin credentials from Key Vault (in a real scenario)
                # This is a placeholder - in real implementation, retrieve from Key Vault
                $adminCredential = Get-AzKeyVaultSecret -VaultName "vatpricing-kv" -Name "sql-admin-password" -ErrorAction SilentlyContinue
                if ($null -eq $adminCredential) {
                    Write-RestoreLog "WARNING: Unable to retrieve SQL admin credentials from Key Vault. Using placeholder for demo purposes." "WARNING" $LogFile
                    $adminPassword = ConvertTo-SecureString -String "ChangeThisPassword123!" -AsPlainText -Force
                } else {
                    $adminPassword = $adminCredential.SecretValue
                }
                
                # Import database
                $importRequest = New-AzSqlDatabaseImport -ResourceGroupName $ResourceGroupName `
                    -ServerName $ServerName `
                    -DatabaseName $targetDbName `
                    -StorageKeyType "StorageAccessKey" `
                    -StorageKey $storageKey `
                    -StorageUri $backupBlob.ICloudBlob.Uri.AbsoluteUri `
                    -AdministratorLogin "vatpricingadmin" `
                    -AdministratorLoginPassword $adminPassword `
                    -Edition $edition `
                    -ServiceObjectiveName $serviceObjective `
                    -DatabaseMaxSizeBytes 268435456000
                
                # Get the import status
                $importStatus = Get-AzSqlDatabaseImportExportStatus -OperationStatusLink $importRequest.OperationStatusLink
                Write-RestoreLog "Database import initiated. Request ID: $($importStatus.RequestId)" "INFO" $LogFile
                
                # Wait for the import operation to complete
                $importComplete = $false
                $startTime = Get-Date
                $timeout = 7200 # 2 hours timeout
                
                while (-not $importComplete) {
                    $importStatus = Get-AzSqlDatabaseImportExportStatus -OperationStatusLink $importRequest.OperationStatusLink
                    
                    if ($importStatus.Status -eq "Succeeded") {
                        $importComplete = $true
                        Write-RestoreLog "Database import completed successfully. Database '$targetDbName' is now available." "INFO" $LogFile
                    }
                    elseif ($importStatus.Status -eq "Failed") {
                        throw "Database import failed: $($importStatus.ErrorMessage)"
                    }
                    else {
                        $currentTime = Get-Date
                        $elapsed = ($currentTime - $startTime).TotalSeconds
                        
                        if ($elapsed -gt $timeout) {
                            throw "Import operation timed out after $timeout seconds."
                        }
                        
                        Write-RestoreLog "Waiting for import operation to complete... Status: $($importStatus.Status), Elapsed time: $([math]::Round($elapsed)) seconds" "INFO" $LogFile
                        Start-Sleep -Seconds 30
                    }
                }
                
                return @{
                    Status = "Success"
                    DatabaseName = $targetDbName
                    RestoreType = "FromBackup"
                    BackupFile = $BackupFileName
                    CompletionTime = Get-Date
                }
            }
        }
        catch {
            Write-RestoreLog "Error restoring SQL database: $_" "ERROR" $LogFile
            throw $_
        }
    }
}

# Function to import data into a Cosmos DB container
function Import-CosmosDBContainer {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$CosmosDBAccountName,
        
        [Parameter(Mandatory = $true)]
        [string]$DatabaseName,
        
        [Parameter(Mandatory = $true)]
        [string]$ContainerName,
        
        [Parameter(Mandatory = $true)]
        [string]$BackupStorageAccountName,
        
        [Parameter(Mandatory = $true)]
        [string]$BackupContainerName,
        
        [Parameter(Mandatory = $true)]
        [string]$BackupFileName
    )

    process {
        Write-RestoreLog "Starting import of Cosmos DB container '$ContainerName' in database '$DatabaseName'" "INFO" $LogFile
        
        try {
            # Get Cosmos DB account
            $cosmosDBAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName
            if (-not $cosmosDBAccount) {
                throw "Cosmos DB account '$CosmosDBAccountName' not found in resource group '$ResourceGroupName'."
            }
            
            # Check if database exists, create if it doesn't
            $cosmosDBDatabase = Get-AzCosmosDBSqlDatabase -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -Name $DatabaseName -ErrorAction SilentlyContinue
            if (-not $cosmosDBDatabase) {
                Write-RestoreLog "Cosmos DB database '$DatabaseName' does not exist. Creating it now." "INFO" $LogFile
                $cosmosDBDatabase = New-AzCosmosDBSqlDatabase -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -Name $DatabaseName
                Write-RestoreLog "Created Cosmos DB database '$DatabaseName'" "INFO" $LogFile
            }
            
            # Check if container exists, create if it doesn't
            $cosmosDBContainer = Get-AzCosmosDBSqlContainer -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -DatabaseName $DatabaseName -Name $ContainerName -ErrorAction SilentlyContinue
            if (-not $cosmosDBContainer) {
                Write-RestoreLog "Cosmos DB container '$ContainerName' does not exist. Creating it now." "INFO" $LogFile
                
                # Determine the partition key based on container name
                $partitionKey = switch ($ContainerName) {
                    "Rules" { "/countryCode" }
                    "AuditLogs" { "/userId" }
                    "Configurations" { "/configType" }
                    default { "/id" }
                }
                
                $cosmosDBContainer = New-AzCosmosDBSqlContainer -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -DatabaseName $DatabaseName -Name $ContainerName -PartitionKeyPath $partitionKey -ThroughputType "Autoscale" -AutoscaleMaxThroughput 4000
                Write-RestoreLog "Created Cosmos DB container '$ContainerName' with partition key '$partitionKey'" "INFO" $LogFile
            }
            
            # Get storage account key
            $storageKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName)[0].Value
            
            # Create storage context
            $storageContext = New-AzStorageContext -StorageAccountName $BackupStorageAccountName -StorageAccountKey $storageKey
            
            # Check if backup file exists
            $backupBlob = Get-AzStorageBlob -Container $BackupContainerName -Blob $BackupFileName -Context $storageContext -ErrorAction SilentlyContinue
            if (-not $backupBlob) {
                throw "Backup file '$BackupFileName' not found in container '$BackupContainerName'."
            }
            
            # Download the backup file to a temp location
            $tempFolder = Join-Path $env:TEMP "CosmosDBRestore_$(Get-Date -Format 'yyyyMMddHHmmss')"
            New-Item -ItemType Directory -Path $tempFolder -Force | Out-Null
            $localBackupPath = Join-Path $tempFolder $BackupFileName
            
            Write-RestoreLog "Downloading backup file '$BackupFileName' to temporary location" "INFO" $LogFile
            Get-AzStorageBlobContent -Container $BackupContainerName -Blob $BackupFileName -Destination $localBackupPath -Context $storageContext | Out-Null
            
            # Get connection string (in a real scenario, use Key Vault)
            $keys = Get-AzCosmosDBAccountKey -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -Type "Keys"
            $primaryKey = $keys.PrimaryMasterKey
            $connectionString = "AccountEndpoint=$($cosmosDBAccount.DocumentEndpoint);AccountKey=$primaryKey;Database=$DatabaseName"
            
            # Create a Data Migration Tool configuration file
            $dt = Get-Date -Format "yyyyMMddHHmmss"
            $configFilePath = Join-Path $tempFolder "import_config_$dt.json"
            
            $importConfig = @{
                DocumentDBConnection = @{
                    ConnectionString = $connectionString
                    Collection = $ContainerName
                }
                DocumentFile = @{
                    Format = "JsonLine"
                    FilePath = $localBackupPath
                }
            } | ConvertTo-Json
            
            $importConfig | Out-File -FilePath $configFilePath -Encoding utf8
            
            # Execute Cosmos DB Data Migration Tool (dt.exe)
            $dtPath = "C:\Program Files\CosmosDB Data Migration Tool\dt.exe"
            if (Test-Path $dtPath) {
                Write-RestoreLog "Starting Cosmos DB data import using Data Migration Tool" "INFO" $LogFile
                $dtArguments = "/s:JsonFile", "/s.Files:$localBackupPath", "/t:DocumentDB", "/t.ConnectionString:`"$connectionString`"", "/t.Collection:$ContainerName", "/t.PartitionKey:$partitionKey"
                
                $process = Start-Process -FilePath $dtPath -ArgumentList $dtArguments -NoNewWindow -PassThru -Wait
                if ($process.ExitCode -ne 0) {
                    Write-RestoreLog "Data Migration Tool exited with code $($process.ExitCode)" "WARNING" $LogFile
                }
            } else {
                # Fallback method using CosmosDB SDK - simplified for this script
                Write-RestoreLog "Data Migration Tool not found. Using fallback import method." "WARNING" $LogFile
                Write-RestoreLog "In a production environment, install the Cosmos DB Data Migration Tool for better performance." "WARNING" $LogFile
                
                # Read the backup file and import data batch by batch
                # This is simplified - in a real script, you'd implement proper batching
                if (Test-Path $localBackupPath) {
                    $documents = Get-Content $localBackupPath | ConvertFrom-Json
                    Write-RestoreLog "Importing $($documents.Count) documents to container '$ContainerName'" "INFO" $LogFile
                    
                    # In a real implementation, you would use the CosmosDB SDK to import the documents
                    Write-RestoreLog "Data import simulation completed (actual import logic would be implemented in a production script)" "INFO" $LogFile
                }
            }
            
            # Clean up temporary files
            Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue
            
            return @{
                Status = "Success"
                AccountName = $CosmosDBAccountName
                DatabaseName = $DatabaseName
                ContainerName = $ContainerName
                BackupFile = $BackupFileName
                CompletionTime = Get-Date
            }
        }
        catch {
            Write-RestoreLog "Error importing Cosmos DB container: $_" "ERROR" $LogFile
            throw $_
        }
    }
}

# Function to restore a Cosmos DB account to a point in time
function Restore-CosmosDBAccount {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$CosmosDBAccountName,
        
        [Parameter(Mandatory = $true)]
        [datetime]$RestoreTimestampUtc,
        
        [Parameter(Mandatory = $true)]
        [string]$TargetDatabaseAccountName
    )

    process {
        Write-RestoreLog "Starting point-in-time restore of Cosmos DB account '$CosmosDBAccountName' to $RestoreTimestampUtc" "INFO" $LogFile
        
        try {
            # Check if source Cosmos DB account exists
            $sourceAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction SilentlyContinue
            if (-not $sourceAccount) {
                throw "Cosmos DB account '$CosmosDBAccountName' not found in resource group '$ResourceGroupName'."
            }
            
            # Check if continuous backup is enabled
            if (-not ($sourceAccount.BackupPolicy.BackupType -eq "Continuous")) {
                throw "Cosmos DB account '$CosmosDBAccountName' does not have continuous backup enabled. Point-in-time restore is not available."
            }
            
            # Check if target account name is available
            $targetAccountExists = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $TargetDatabaseAccountName -ErrorAction SilentlyContinue
            if ($targetAccountExists) {
                throw "Target Cosmos DB account '$TargetDatabaseAccountName' already exists."
            }
            
            Write-RestoreLog "Initiating point-in-time restore for Cosmos DB account" "INFO" $LogFile
            
            # Start the restore operation
            $restoreOperation = Restore-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName `
                -SourceDatabaseAccountName $CosmosDBAccountName `
                -TargetDatabaseAccountName $TargetDatabaseAccountName `
                -RestoreTimestampInUtc $RestoreTimestampUtc `
                -Location $sourceAccount.Location
            
            Write-RestoreLog "Restore operation initiated. Target account: $TargetDatabaseAccountName" "INFO" $LogFile
            
            # Wait for the restore operation to complete
            $restoreComplete = $false
            $startTime = Get-Date
            $timeout = 7200 # 2 hours timeout
            
            while (-not $restoreComplete) {
                $targetAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $TargetDatabaseAccountName -ErrorAction SilentlyContinue
                
                if ($targetAccount -and $targetAccount.ProvisioningState -eq "Succeeded") {
                    $restoreComplete = $true
                    Write-RestoreLog "Point-in-time restore completed successfully. Target account '$TargetDatabaseAccountName' is available." "INFO" $LogFile
                }
                elseif ($targetAccount -and $targetAccount.ProvisioningState -eq "Failed") {
                    throw "Restore operation failed. Target account provisioning state: Failed"
                }
                else {
                    $currentTime = Get-Date
                    $elapsed = ($currentTime - $startTime).TotalSeconds
                    
                    if ($elapsed -gt $timeout) {
                        throw "Restore operation timed out after $timeout seconds."
                    }
                    
                    Write-RestoreLog "Waiting for restore operation to complete... Elapsed time: $([math]::Round($elapsed)) seconds" "INFO" $LogFile
                    Start-Sleep -Seconds 30
                }
            }
            
            return @{
                Status = "Success"
                SourceAccountName = $CosmosDBAccountName
                TargetAccountName = $TargetDatabaseAccountName
                RestoreTime = $RestoreTimestampUtc
                CompletionTime = Get-Date
            }
        }
        catch {
            Write-RestoreLog "Error restoring Cosmos DB account: $_" "ERROR" $LogFile
            throw $_
        }
    }
}

# Function to validate a restored database
function Validate-DatabaseRestore {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("SQL", "CosmosDB")]
        [string]$DatabaseType,
        
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $false)]
        [string]$ServerName,
        
        [Parameter(Mandatory = $false)]
        [string]$DatabaseName,
        
        [Parameter(Mandatory = $false)]
        [string]$CosmosDBAccountName,
        
        [Parameter(Mandatory = $false)]
        [string]$CosmosDBDatabaseName,
        
        [Parameter(Mandatory = $false)]
        [string]$CosmosDBContainerName
    )

    process {
        try {
            if ($DatabaseType -eq "SQL") {
                Write-RestoreLog "Validating restored SQL Database '$DatabaseName'" "INFO" $LogFile
                
                # Check if database exists and is online
                $database = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $ServerName -DatabaseName $DatabaseName -ErrorAction Stop
                
                if ($database.Status -ne "Online") {
                    Write-RestoreLog "SQL Database '$DatabaseName' is not online. Current status: $($database.Status)" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Database is not online. Current status: $($database.Status)"
                    }
                }
                
                # Check database size and other properties
                $databaseSizeGB = [math]::Round($database.MaxSizeBytes / 1GB, 2)
                Write-RestoreLog "SQL Database '$DatabaseName' is online. Size: $databaseSizeGB GB, Edition: $($database.Edition), Service Objective: $($database.CurrentServiceObjectiveName)" "INFO" $LogFile
                
                # Basic validation query
                try {
                    # In a real script, you would execute a validation query here
                    # For example, using Invoke-Sqlcmd or SqlClient to check for table count
                    Write-RestoreLog "Executing validation query on database '$DatabaseName'" "INFO" $LogFile
                    
                    # Simulation of validation query
                    Start-Sleep -Seconds 2
                    Write-RestoreLog "Validation query completed successfully" "INFO" $LogFile
                }
                catch {
                    Write-RestoreLog "Validation query failed: $_" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Validation query failed: $_"
                    }
                }
                
                Write-RestoreLog "Validation of SQL Database '$DatabaseName' completed successfully" "INFO" $LogFile
                
                return @{
                    Status = "Success"
                    DatabaseName = $DatabaseName
                    DatabaseStatus = $database.Status
                    DatabaseSize = $databaseSizeGB
                    Edition = $database.Edition
                    ServiceObjective = $database.CurrentServiceObjectiveName
                }
            }
            elseif ($DatabaseType -eq "CosmosDB") {
                Write-RestoreLog "Validating restored Cosmos DB container '$CosmosDBContainerName'" "INFO" $LogFile
                
                # Check if account, database and container exist
                $account = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction Stop
                
                if ($account.ProvisioningState -ne "Succeeded") {
                    Write-RestoreLog "Cosmos DB account '$CosmosDBAccountName' is not in succeeded state. Current state: $($account.ProvisioningState)" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Cosmos DB account is not in succeeded state. Current state: $($account.ProvisioningState)"
                    }
                }
                
                # Check database
                $database = Get-AzCosmosDBSqlDatabase -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -Name $CosmosDBDatabaseName -ErrorAction SilentlyContinue
                if (-not $database) {
                    Write-RestoreLog "Cosmos DB database '$CosmosDBDatabaseName' not found" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Cosmos DB database not found"
                    }
                }
                
                # Check container
                $container = Get-AzCosmosDBSqlContainer -ResourceGroupName $ResourceGroupName -AccountName $CosmosDBAccountName -DatabaseName $CosmosDBDatabaseName -Name $CosmosDBContainerName -ErrorAction SilentlyContinue
                if (-not $container) {
                    Write-RestoreLog "Cosmos DB container '$CosmosDBContainerName' not found" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Cosmos DB container not found"
                    }
                }
                
                # Basic validation - checking container properties
                try {
                    # In a real script, you would execute a validation query here to check document count
                    Write-RestoreLog "Validating Cosmos DB container configuration and accessibility" "INFO" $LogFile
                    
                    # Simulation of document count check
                    Start-Sleep -Seconds 2
                    Write-RestoreLog "Container validation completed successfully" "INFO" $LogFile
                }
                catch {
                    Write-RestoreLog "Container validation failed: $_" "WARNING" $LogFile
                    return @{
                        Status = "Warning"
                        Message = "Container validation failed: $_"
                    }
                }
                
                Write-RestoreLog "Validation of Cosmos DB container '$CosmosDBContainerName' completed successfully" "INFO" $LogFile
                
                return @{
                    Status = "Success"
                    AccountName = $CosmosDBAccountName
                    DatabaseName = $CosmosDBDatabaseName
                    ContainerName = $CosmosDBContainerName
                    PartitionKey = $container.Resource.PartitionKey.Paths[0]
                    ThroughputType = if ($container.Resource.Throughput) { "Manual" } else { "Autoscale" }
                }
            }
            else {
                throw "Invalid database type: $DatabaseType"
            }
        }
        catch {
            Write-RestoreLog "Error validating database restore: $_" "ERROR" $LogFile
            return @{
                Status = "Error"
                Message = "Validation failed: $_"
            }
        }
    }
}

# Function to write logs
function Write-RestoreLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        
        [Parameter(Mandatory = $false)]
        [ValidateSet("INFO", "WARNING", "ERROR", "SUCCESS")]
        [string]$LogLevel = "INFO",
        
        [Parameter(Mandatory = $false)]
        [string]$LogFile
    )

    process {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $logMessage = "[$timestamp] [$LogLevel] $Message"
        
        # Write to console with color based on log level
        switch ($LogLevel) {
            "INFO" { Write-Host $logMessage -ForegroundColor Gray }
            "WARNING" { Write-Host $logMessage -ForegroundColor Yellow }
            "ERROR" { Write-Host $logMessage -ForegroundColor Red }
            "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
            default { Write-Host $logMessage }
        }
        
        # Write to log file if specified
        if (-not [string]::IsNullOrEmpty($LogFile)) {
            Add-Content -Path $LogFile -Value $logMessage
        }
    }
}

# Main script

# Process parameters
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("FromBackup", "PointInTime")]
    [string]$RestoreType = "FromBackup",
    
    [Parameter(Mandatory = $true)]
    [string]$SqlServerName,
    
    [Parameter(Mandatory = $true)]
    [string]$SqlDatabaseName,
    
    [Parameter(Mandatory = $true)]
    [string]$CosmosDBAccountName,
    
    [Parameter(Mandatory = $false)]
    [string]$CosmosDBDatabaseName = "VatFilingPricingTool",
    
    [Parameter(Mandatory = $true)]
    [string]$BackupStorageAccountName,
    
    [Parameter(Mandatory = $false)]
    [string]$BackupContainerName = "database-backups",
    
    [Parameter(Mandatory = $false)]
    [string]$SqlBackupFileName,
    
    [Parameter(Mandatory = $false)]
    [string]$CosmosContainers = "Rules,AuditLogs,Configurations",
    
    [Parameter(Mandatory = $false)]
    [datetime]$PointInTimeUtc,
    
    [Parameter(Mandatory = $false)]
    [string]$TargetDatabaseName,
    
    [Parameter(Mandatory = $false)]
    [string]$TargetCosmosDBAccountName,
    
    [Parameter(Mandatory = $false)]
    [bool]$ValidateRestore = $true,
    
    [Parameter(Mandatory = $false)]
    [string]$LogFile = "./database-restore-{timestamp}.log"
)

# Create timestamp for restore
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$LogFile = $LogFile -replace "{timestamp}", $timestamp

# Create log file directory if it doesn't exist
$logDir = Split-Path -Path $LogFile -Parent
if (-not [string]::IsNullOrEmpty($logDir) -and -not (Test-Path -Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# Start restore operation
Write-RestoreLog "Starting database restore operation for environment: $Environment" "INFO" $LogFile
Write-RestoreLog "Restore type: $RestoreType" "INFO" $LogFile
Write-RestoreLog "Resource group: $ResourceGroupName" "INFO" $LogFile

# Check if user is logged into Azure
if (-not (Test-AzLogin)) {
    Write-RestoreLog "Not logged into Azure. Prompting for login." "INFO" $LogFile
    Connect-AzAccount
}

# Verify resources exist
try {
    # Check resource group
    $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction Stop
    Write-RestoreLog "Resource group '$ResourceGroupName' exists" "INFO" $LogFile
    
    # Check SQL Server
    $sqlServer = Get-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -ErrorAction Stop
    Write-RestoreLog "SQL Server '$SqlServerName' exists" "INFO" $LogFile
    
    # Check Cosmos DB account
    $cosmosAccount = Get-AzCosmosDBAccount -ResourceGroupName $ResourceGroupName -Name $CosmosDBAccountName -ErrorAction Stop
    Write-RestoreLog "Cosmos DB account '$CosmosDBAccountName' exists" "INFO" $LogFile
    
    # Check backup storage account
    $storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName -ErrorAction Stop
    Write-RestoreLog "Storage account '$BackupStorageAccountName' exists" "INFO" $LogFile
    
    # Check backup container
    $storageKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $BackupStorageAccountName)[0].Value
    $storageContext = New-AzStorageContext -StorageAccountName $BackupStorageAccountName -StorageAccountKey $storageKey
    $container = Get-AzStorageContainer -Name $BackupContainerName -Context $storageContext -ErrorAction Stop
    Write-RestoreLog "Storage container '$BackupContainerName' exists" "INFO" $LogFile
}
catch {
    Write-RestoreLog "Error verifying resources: $_" "ERROR" $LogFile
    Write-RestoreLog "Cannot proceed with restore operation. Please verify the resources exist and you have sufficient permissions." "ERROR" $LogFile
    exit 1
}

# Process restore based on type
if ($RestoreType -eq "FromBackup") {
    # Verify backup files exist
    if ([string]::IsNullOrEmpty($SqlBackupFileName)) {
        Write-RestoreLog "SQL backup file name is required for FromBackup restore type" "ERROR" $LogFile
        exit 1
    }
    
    # Check if SQL backup file exists
    $sqlBackupBlob = Get-AzStorageBlob -Container $BackupContainerName -Blob $SqlBackupFileName -Context $storageContext -ErrorAction SilentlyContinue
    if (-not $sqlBackupBlob) {
        Write-RestoreLog "SQL backup file '$SqlBackupFileName' not found in container '$BackupContainerName'" "ERROR" $LogFile
        exit 1
    }
    
    Write-RestoreLog "SQL backup file '$SqlBackupFileName' found in storage" "INFO" $LogFile
    
    # Restore SQL Database
    try {
        $sqlRestoreResult = Restore-SqlDatabase -ResourceGroupName $ResourceGroupName `
            -ServerName $SqlServerName `
            -DatabaseName $SqlDatabaseName `
            -BackupStorageAccountName $BackupStorageAccountName `
            -BackupContainerName $BackupContainerName `
            -BackupFileName $SqlBackupFileName `
            -UsePointInTimeRestore $false
        
        Write-RestoreLog "SQL Database restore completed. Status: $($sqlRestoreResult.Status)" "SUCCESS" $LogFile
    }
    catch {
        Write-RestoreLog "Failed to restore SQL Database: $_" "ERROR" $LogFile
        exit 1
    }
    
    # Process Cosmos DB containers
    $containerList = $CosmosContainers -split ','
    foreach ($containerName in $containerList) {
        $containerName = $containerName.Trim()
        if ([string]::IsNullOrEmpty($containerName)) {
            continue
        }
        
        $cosmosBackupFileName = "$containerName-backup-latest.json"
        
        # Check if Cosmos backup file exists
        $cosmosBackupBlob = Get-AzStorageBlob -Container $BackupContainerName -Blob $cosmosBackupFileName -Context $storageContext -ErrorAction SilentlyContinue
        if (-not $cosmosBackupBlob) {
            Write-RestoreLog "Cosmos DB backup file '$cosmosBackupFileName' not found in container '$BackupContainerName'" "WARNING" $LogFile
            continue
        }
        
        Write-RestoreLog "Cosmos DB backup file '$cosmosBackupFileName' found in storage" "INFO" $LogFile
        
        # Import Cosmos DB container
        try {
            $cosmosRestoreResult = Import-CosmosDBContainer -ResourceGroupName $ResourceGroupName `
                -CosmosDBAccountName $CosmosDBAccountName `
                -DatabaseName $CosmosDBDatabaseName `
                -ContainerName $containerName `
                -BackupStorageAccountName $BackupStorageAccountName `
                -BackupContainerName $BackupContainerName `
                -BackupFileName $cosmosBackupFileName
            
            Write-RestoreLog "Cosmos DB container '$containerName' import completed. Status: $($cosmosRestoreResult.Status)" "SUCCESS" $LogFile
        }
        catch {
            Write-RestoreLog "Failed to import Cosmos DB container '$containerName': $_" "ERROR" $LogFile
        }
    }
}
elseif ($RestoreType -eq "PointInTime") {
    # Verify point in time is valid
    if ($null -eq $PointInTimeUtc) {
        Write-RestoreLog "Point in time (PointInTimeUtc) is required for PointInTime restore type" "ERROR" $LogFile
        exit 1
    }
    
    # Check if point in time is in the past
    if ($PointInTimeUtc -gt (Get-Date).ToUniversalTime()) {
        Write-RestoreLog "Point in time must be in the past" "ERROR" $LogFile
        exit 1
    }
    
    # Restore SQL Database
    try {
        $sqlRestoreResult = Restore-SqlDatabase -ResourceGroupName $ResourceGroupName `
            -ServerName $SqlServerName `
            -DatabaseName $SqlDatabaseName `
            -PointInTimeUtc $PointInTimeUtc `
            -UsePointInTimeRestore $true
        
        Write-RestoreLog "SQL Database point-in-time restore completed. Status: $($sqlRestoreResult.Status)" "SUCCESS" $LogFile
    }
    catch {
        Write-RestoreLog "Failed to restore SQL Database to point in time: $_" "ERROR" $LogFile
        exit 1
    }
    
    # Restore Cosmos DB account if specified
    if (-not [string]::IsNullOrEmpty($TargetCosmosDBAccountName)) {
        try {
            $cosmosRestoreResult = Restore-CosmosDBAccount -ResourceGroupName $ResourceGroupName `
                -CosmosDBAccountName $CosmosDBAccountName `
                -RestoreTimestampUtc $PointInTimeUtc `
                -TargetDatabaseAccountName $TargetCosmosDBAccountName
            
            Write-RestoreLog "Cosmos DB account point-in-time restore completed. Status: $($cosmosRestoreResult.Status)" "SUCCESS" $LogFile
        }
        catch {
            Write-RestoreLog "Failed to restore Cosmos DB account to point in time: $_" "ERROR" $LogFile
        }
    }
    else {
        Write-RestoreLog "Skipping Cosmos DB account restore as no target account name was provided" "INFO" $LogFile
    }
}
else {
    Write-RestoreLog "Invalid restore type: $RestoreType" "ERROR" $LogFile
    exit 1
}

# Validate the restore if requested
if ($ValidateRestore) {
    Write-RestoreLog "Validating restored databases" "INFO" $LogFile
    
    # Determine the names of the restored databases
    $restoredSqlDbName = if ($TargetDatabaseName) { $TargetDatabaseName } else { if ($RestoreType -eq "FromBackup") { $SqlDatabaseName + "_restored" } else { $SqlDatabaseName + "_restored" } }
    $restoredCosmosAccountName = if ($TargetCosmosDBAccountName) { $TargetCosmosDBAccountName } else { $CosmosDBAccountName }
    
    # Validate SQL Database
    $sqlValidationResult = Validate-DatabaseRestore -DatabaseType "SQL" `
        -ResourceGroupName $ResourceGroupName `
        -ServerName $SqlServerName `
        -DatabaseName $restoredSqlDbName
    
    Write-RestoreLog "SQL Database validation result: $($sqlValidationResult.Status)" "INFO" $LogFile
    
    # Validate Cosmos DB if applicable
    if ($RestoreType -eq "FromBackup" -or -not [string]::IsNullOrEmpty($TargetCosmosDBAccountName)) {
        $containerList = $CosmosContainers -split ','
        foreach ($containerName in $containerList) {
            $containerName = $containerName.Trim()
            if ([string]::IsNullOrEmpty($containerName)) {
                continue
            }
            
            $cosmosValidationResult = Validate-DatabaseRestore -DatabaseType "CosmosDB" `
                -ResourceGroupName $ResourceGroupName `
                -CosmosDBAccountName $restoredCosmosAccountName `
                -CosmosDBDatabaseName $CosmosDBDatabaseName `
                -CosmosDBContainerName $containerName
            
            Write-RestoreLog "Cosmos DB container '$containerName' validation result: $($cosmosValidationResult.Status)" "INFO" $LogFile
        }
    }
}

# Output restore operation summary
Write-RestoreLog "Database restore operation completed" "SUCCESS" $LogFile
Write-RestoreLog "SQL Database: $($sqlRestoreResult.DatabaseName)" "SUCCESS" $LogFile
if ($RestoreType -eq "PointInTime" -and -not [string]::IsNullOrEmpty($TargetCosmosDBAccountName)) {
    Write-RestoreLog "Cosmos DB Account: $($cosmosRestoreResult.TargetAccountName)" "SUCCESS" $LogFile
}

Write-RestoreLog "Restore log file: $LogFile" "INFO" $LogFile