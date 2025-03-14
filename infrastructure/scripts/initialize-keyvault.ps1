<#
.SYNOPSIS
    Initializes Azure Key Vault with necessary secrets for the VAT Filing Pricing Tool.

.DESCRIPTION
    This script creates and configures secrets in Azure Key Vault required by the VAT Filing Pricing Tool application,
    including database connection strings, API keys, and other sensitive configuration values.
    
.PARAMETER KeyVaultName
    Name of the Azure Key Vault to initialize.

.PARAMETER ResourceGroupName
    Name of the resource group containing the Key Vault.

.PARAMETER SqlConnectionString
    Connection string for Azure SQL Database.

.PARAMETER CosmosDbConnectionString
    Connection string for Azure Cosmos DB.

.PARAMETER StorageConnectionString
    Connection string for Azure Storage Account.

.PARAMETER RedisConnectionString
    Connection string for Azure Redis Cache.

.PARAMETER SqlAdminUsername
    Admin username for SQL Server.

.PARAMETER SqlAdminPassword
    Admin password for SQL Server.

.PARAMETER AzureAdClientId
    Client ID for Azure AD application.

.PARAMETER AzureAdClientSecret
    Client secret for Azure AD application.

.PARAMETER AzureAdTenantId
    Tenant ID for Azure AD.

.PARAMETER GenerateRandomPasswords
    Switch to generate random passwords for missing secrets.

.PARAMETER SecretsHashtable
    Hashtable of secret names and values to set in Key Vault.

.EXAMPLE
    .\initialize-keyvault.ps1 -KeyVaultName "vat-tool-kv" -ResourceGroupName "vat-tool-rg" -GenerateRandomPasswords

.EXAMPLE
    .\initialize-keyvault.ps1 -KeyVaultName "vat-tool-kv" -ResourceGroupName "vat-tool-rg" -SqlConnectionString "Server=server.database.windows.net;Database=vatdb;User ID=admin;Password=pwd;"

.NOTES
    File Name      : initialize-keyvault.ps1
    Prerequisite   : Az PowerShell module installed, Azure subscription connected
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory = $false)]
    [string]$SqlConnectionString,
    
    [Parameter(Mandatory = $false)]
    [string]$CosmosDbConnectionString,
    
    [Parameter(Mandatory = $false)]
    [string]$StorageConnectionString,
    
    [Parameter(Mandatory = $false)]
    [string]$RedisConnectionString,
    
    [Parameter(Mandatory = $false)]
    [string]$SqlAdminUsername,
    
    [Parameter(Mandatory = $false)]
    [string]$SqlAdminPassword,
    
    [Parameter(Mandatory = $false)]
    [string]$AzureAdClientId,
    
    [Parameter(Mandatory = $false)]
    [string]$AzureAdClientSecret,
    
    [Parameter(Mandatory = $false)]
    [string]$AzureAdTenantId,
    
    [Parameter(Mandatory = $false)]
    [switch]$GenerateRandomPasswords,
    
    [Parameter(Mandatory = $false)]
    [hashtable]$SecretsHashtable
)

# Set strict error handling
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

# Function to set a secret in Azure Key Vault
function Set-KeyVaultSecret {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        
        [Parameter(Mandatory = $true)]
        [string]$SecretName,
        
        [Parameter(Mandatory = $true)]
        [string]$SecretValue,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$Tags
    )
    
    try {
        if ([string]::IsNullOrEmpty($KeyVaultName)) {
            throw "KeyVaultName parameter is required"
        }
        
        if ([string]::IsNullOrEmpty($SecretName)) {
            throw "SecretName parameter is required"
        }
        
        if ([string]::IsNullOrEmpty($SecretValue)) {
            throw "SecretValue parameter is required"
        }
        
        $secureValue = ConvertTo-SecureString -String $SecretValue -AsPlainText -Force
        
        $params = @{
            VaultName   = $KeyVaultName
            Name        = $SecretName
            SecretValue = $secureValue
        }
        
        if ($Tags -and $Tags.Count -gt 0) {
            $params.Add('Tag', $Tags)
        }
        
        $secret = Set-AzKeyVaultSecret @params
        Write-Verbose "Secret '$SecretName' has been set in Key Vault '$KeyVaultName'"
        return $true
    }
    catch {
        Write-Error "Error setting secret '$SecretName' in Key Vault '$KeyVaultName': $_"
        return $false
    }
}

# Function to get a secret from Azure Key Vault
function Get-KeyVaultSecret {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        
        [Parameter(Mandatory = $true)]
        [string]$SecretName
    )
    
    try {
        if ([string]::IsNullOrEmpty($KeyVaultName)) {
            throw "KeyVaultName parameter is required"
        }
        
        if ([string]::IsNullOrEmpty($SecretName)) {
            throw "SecretName parameter is required"
        }
        
        $secret = Get-AzKeyVaultSecret -VaultName $KeyVaultName -Name $SecretName
        
        if ($null -eq $secret) {
            return $null
        }
        
        # Convert SecureString to plain text
        $plainTextSecret = ConvertFrom-SecureString -SecureString $secret.SecretValue -AsPlainText
        
        return $plainTextSecret
    }
    catch {
        Write-Error "Error getting secret '$SecretName' from Key Vault '$KeyVaultName': $_"
        return $null
    }
}

# Function to check if a secret exists in Azure Key Vault
function Test-KeyVaultSecretExists {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        
        [Parameter(Mandatory = $true)]
        [string]$SecretName
    )
    
    try {
        if ([string]::IsNullOrEmpty($KeyVaultName)) {
            throw "KeyVaultName parameter is required"
        }
        
        if ([string]::IsNullOrEmpty($SecretName)) {
            throw "SecretName parameter is required"
        }
        
        $secret = Get-AzKeyVaultSecret -VaultName $KeyVaultName -Name $SecretName -ErrorAction SilentlyContinue
        
        return ($null -ne $secret)
    }
    catch [Microsoft.Azure.KeyVault.Models.KeyVaultErrorException] {
        # Secret not found
        return $false
    }
    catch {
        # Rethrow any other errors
        throw
    }
}

# Function to generate a random password
function New-RandomPassword {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [int]$Length = 16,
        
        [Parameter(Mandatory = $false)]
        [bool]$IncludeSpecialCharacters = $true
    )
    
    # Define character sets
    $uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    $lowercase = "abcdefghijklmnopqrstuvwxyz"
    $numbers = "0123456789"
    $special = "!@#$%^&*()-_=+[]{}|;:,.<>?"
    
    # Create character list
    $charList = $uppercase + $lowercase + $numbers
    if ($IncludeSpecialCharacters) {
        $charList += $special
    }
    
    # Use RNGCryptoServiceProvider for secure random number generation
    $rng = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
    $bytes = New-Object byte[]($Length)
    $rng.GetBytes($bytes)
    
    # Convert random bytes to characters
    $password = ""
    for ($i = 0; $i -lt $Length; $i++) {
        $password += $charList[$bytes[$i] % $charList.Length]
    }
    
    # Ensure the password contains at least one character from each required set
    $sets = @($uppercase, $lowercase, $numbers)
    if ($IncludeSpecialCharacters) {
        $sets += $special
    }
    
    foreach ($set in $sets) {
        if (-not ($password.ToCharArray() | Where-Object { $set.Contains($_) })) {
            $randomIndex = Get-Random -Minimum 0 -Maximum $Length
            $randomChar = $set[(Get-Random -Minimum 0 -Maximum $set.Length)]
            $passwordArray = $password.ToCharArray()
            $passwordArray[$randomIndex] = $randomChar
            $password = -join $passwordArray
        }
    }
    
    return $password
}

# Function to initialize Key Vault with required secrets
function Initialize-KeyVault {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [hashtable]$Secrets,
        
        [Parameter(Mandatory = $false)]
        [switch]$GenerateRandomPasswords
    )
    
    try {
        # Validate the Key Vault exists
        $keyVault = Get-AzKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        
        if ($null -eq $keyVault) {
            throw "Key Vault '$KeyVaultName' not found in Resource Group '$ResourceGroupName'"
        }
        
        Write-Verbose "Found Key Vault '$KeyVaultName' in Resource Group '$ResourceGroupName'"
        
        # Initialize result hashtable
        $result = @{}
        
        # Process each secret
        foreach ($key in $Secrets.Keys) {
            $secretName = $key
            $secretValue = $Secrets[$key]
            
            # Check if secret already exists
            $secretExists = Test-KeyVaultSecretExists -KeyVaultName $KeyVaultName -SecretName $secretName
            
            if ($secretExists) {
                Write-Verbose "Secret '$secretName' already exists in Key Vault '$KeyVaultName'"
                # Get the existing secret value
                $secretValue = Get-KeyVaultSecret -KeyVaultName $KeyVaultName -SecretName $secretName
            }
            else {
                # If secret doesn't exist and value is empty, generate a random password if requested
                if ([string]::IsNullOrEmpty($secretValue) -and $GenerateRandomPasswords) {
                    Write-Verbose "Generating random password for secret '$secretName'"
                    $secretValue = New-RandomPassword -Length 24 -IncludeSpecialCharacters $true
                }
                
                # Set the secret in Key Vault
                if (-not [string]::IsNullOrEmpty($secretValue)) {
                    $success = Set-KeyVaultSecret -KeyVaultName $KeyVaultName -SecretName $secretName -SecretValue $secretValue
                    
                    if (-not $success) {
                        Write-Warning "Failed to set secret '$secretName' in Key Vault '$KeyVaultName'"
                    }
                }
                else {
                    Write-Warning "No value provided for secret '$secretName' and random password generation not enabled"
                }
            }
            
            # Add the secret to the result hashtable
            $result[$secretName] = $secretValue
        }
        
        return $result
    }
    catch {
        Write-Error "Error initializing Key Vault: $_"
        throw
    }
}

# Main script execution
try {
    # Import required modules
    try {
        Import-Module -Name Az.Accounts -ErrorAction Stop # v2.12.1
        Import-Module -Name Az.KeyVault -ErrorAction Stop # v4.10.0
        Import-Module -Name Az.Resources -ErrorAction Stop # v6.6.0
    }
    catch {
        Write-Error "Error importing Azure PowerShell modules. Please make sure the Az module is installed."
        Write-Error "You can install it with: Install-Module -Name Az -AllowClobber -Force"
        exit 1
    }

    # Check if connected to Azure
    try {
        $context = Get-AzContext
        if (-not $context) {
            Write-Verbose "Not connected to Azure. Connecting..."
            Connect-AzAccount
        }
        else {
            Write-Verbose "Already connected to Azure as $($context.Account) in subscription $($context.Subscription.Name)"
        }
    }
    catch {
        Write-Error "Error connecting to Azure: $_"
        exit 1
    }

    # Verify Key Vault exists
    try {
        $keyVault = Get-AzKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -ErrorAction Stop
        
        if ($null -eq $keyVault) {
            Write-Error "Key Vault '$KeyVaultName' not found in Resource Group '$ResourceGroupName'"
            exit 1
        }
        
        Write-Verbose "Found Key Vault '$KeyVaultName' in Resource Group '$ResourceGroupName'"
    }
    catch {
        Write-Error "Error verifying Key Vault: $_"
        exit 1
    }

    # Create a hashtable of secrets to initialize
    $secretsToInitialize = @{}

    # Add provided secrets to the hashtable
    if (-not [string]::IsNullOrEmpty($SqlConnectionString)) {
        $secretsToInitialize["SqlConnectionString"] = $SqlConnectionString
    }

    if (-not [string]::IsNullOrEmpty($CosmosDbConnectionString)) {
        $secretsToInitialize["CosmosDbConnectionString"] = $CosmosDbConnectionString
    }

    if (-not [string]::IsNullOrEmpty($StorageConnectionString)) {
        $secretsToInitialize["StorageConnectionString"] = $StorageConnectionString
    }

    if (-not [string]::IsNullOrEmpty($RedisConnectionString)) {
        $secretsToInitialize["RedisConnectionString"] = $RedisConnectionString
    }

    if (-not [string]::IsNullOrEmpty($SqlAdminUsername)) {
        $secretsToInitialize["SqlAdminUsername"] = $SqlAdminUsername
    }

    if (-not [string]::IsNullOrEmpty($SqlAdminPassword)) {
        $secretsToInitialize["SqlAdminPassword"] = $SqlAdminPassword
    }

    if (-not [string]::IsNullOrEmpty($AzureAdClientId)) {
        $secretsToInitialize["AzureAdClientId"] = $AzureAdClientId
    }

    if (-not [string]::IsNullOrEmpty($AzureAdClientSecret)) {
        $secretsToInitialize["AzureAdClientSecret"] = $AzureAdClientSecret
    }

    if (-not [string]::IsNullOrEmpty($AzureAdTenantId)) {
        $secretsToInitialize["AzureAdTenantId"] = $AzureAdTenantId
    }

    # Add standard secret names even if values are not provided
    $standardSecrets = @(
        "SqlConnectionString",
        "CosmosDbConnectionString",
        "StorageConnectionString",
        "RedisConnectionString",
        "SqlAdminUsername",
        "SqlAdminPassword",
        "AzureAdClientId",
        "AzureAdClientSecret",
        "AzureAdTenantId",
        "JwtSigningKey",
        "ApiKey",
        "CognitiveServicesKey",
        "SendGridApiKey",
        "ApplicationInsightsKey"
    )

    foreach ($secretName in $standardSecrets) {
        if (-not $secretsToInitialize.ContainsKey($secretName)) {
            $secretsToInitialize[$secretName] = ""
        }
    }

    # Merge with additional secrets provided in the hashtable parameter
    if ($SecretsHashtable -and $SecretsHashtable.Count -gt 0) {
        foreach ($key in $SecretsHashtable.Keys) {
            $secretsToInitialize[$key] = $SecretsHashtable[$key]
        }
    }

    # Initialize Key Vault with secrets
    $result = Initialize-KeyVault -KeyVaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -Secrets $secretsToInitialize -GenerateRandomPasswords:$GenerateRandomPasswords
    
    # Output summary
    Write-Host ""
    Write-Host "Key Vault Initialization Summary" -ForegroundColor Green
    Write-Host "-------------------------------" -ForegroundColor Green
    Write-Host ""
    
    foreach ($key in $result.Keys | Sort-Object) {
        $valueDisplay = if ($result[$key]) { "Set" } else { "Not set" }
        Write-Host "Secret: $key - $valueDisplay"
    }
    
    Write-Host ""
    Write-Host "Key Vault '$KeyVaultName' has been initialized successfully." -ForegroundColor Green
    
    exit 0
}
catch {
    Write-Error "Error initializing Key Vault: $_"
    exit 1
}

# Export functions for module usage
Export-ModuleMember -Function Set-KeyVaultSecret, Get-KeyVaultSecret, Test-KeyVaultSecretExists, New-RandomPassword, Initialize-KeyVault