<#
.SYNOPSIS
    Deployment script for the VAT Filing Pricing Tool infrastructure.

.DESCRIPTION
    This script orchestrates the deployment of Azure resources using either ARM templates
    or Terraform, initializes Key Vault with required secrets, and configures Kubernetes
    resources for the application.

.PARAMETER DeploymentType
    Type of deployment to perform (ARM or Terraform).

.PARAMETER Environment
    Deployment environment (dev, staging, prod).

.PARAMETER Location
    Primary Azure region for resource deployment.

.PARAMETER SecondaryLocation
    Secondary Azure region for disaster recovery.

.PARAMETER ResourceGroupName
    Name of the resource group to deploy to.

.PARAMETER AppName
    Name of the application.

.PARAMETER SqlAdminUsername
    SQL Server administrator username.

.PARAMETER SqlAdminPassword
    SQL Server administrator password.

.PARAMETER AksNodeCount
    Number of nodes in AKS cluster.

.PARAMETER AksNodeSize
    VM size for AKS nodes.

.PARAMETER DeployKubernetes
    Whether to deploy Kubernetes resources.

.PARAMETER BuildVersion
    Version of the application to deploy.

.PARAMETER AcrName
    Name of the Azure Container Registry.

.PARAMETER TemplateFile
    Path to ARM template file.

.PARAMETER ParameterFile
    Path to ARM parameter file.

.PARAMETER TerraformDirectory
    Path to Terraform configuration directory.

.PARAMETER TerraformVarFile
    Path to Terraform variables file.

.PARAMETER KubernetesDirectory
    Path to Kubernetes manifest directory.

.PARAMETER HelmDirectory
    Path to Helm chart directory.

.EXAMPLE
    .\deploy.ps1 -DeploymentType ARM -Environment dev -Location westeurope

.EXAMPLE
    .\deploy.ps1 -DeploymentType Terraform -Environment staging -Location westeurope -SecondaryLocation northeurope -DeployKubernetes $true

.NOTES
    File Name      : deploy.ps1
    Prerequisite   : Az PowerShell modules, Terraform CLI, kubectl CLI, Helm CLI
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [ValidateSet("ARM", "Terraform")]
    [string]$DeploymentType = "ARM",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "westeurope",
    
    [Parameter(Mandatory = $false)]
    [string]$SecondaryLocation = "northeurope",
    
    [Parameter(Mandatory = $false)]
    [string]$ResourceGroupName = "rg-vatfilingpricingtool-$Environment",
    
    [Parameter(Mandatory = $false)]
    [string]$AppName = "vatfilingpricingtool",
    
    [Parameter(Mandatory = $false)]
    [string]$SqlAdminUsername = "sqladmin",
    
    [Parameter(Mandatory = $false)]
    [SecureString]$SqlAdminPassword,
    
    [Parameter(Mandatory = $false)]
    [int]$AksNodeCount = 3,
    
    [Parameter(Mandatory = $false)]
    [string]$AksNodeSize = "Standard_D4s_v3",
    
    [Parameter(Mandatory = $false)]
    [bool]$DeployKubernetes = $true,
    
    [Parameter(Mandatory = $false)]
    [string]$BuildVersion = "latest",
    
    [Parameter(Mandatory = $false)]
    [string]$AcrName = "",
    
    [Parameter(Mandatory = $false)]
    [string]$TemplateFile = "../azure/arm-templates/main.json",
    
    [Parameter(Mandatory = $false)]
    [string]$ParameterFile = "../azure/arm-templates/parameters.json",
    
    [Parameter(Mandatory = $false)]
    [string]$TerraformDirectory = "../azure/terraform",
    
    [Parameter(Mandatory = $false)]
    [string]$TerraformVarFile = "../azure/terraform/environments/$Environment/terraform.tfvars",
    
    [Parameter(Mandatory = $false)]
    [string]$KubernetesDirectory = "../kubernetes",
    
    [Parameter(Mandatory = $false)]
    [string]$HelmDirectory = "../helm/vatfilingpricingtool"
)

# Set strict error handling
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

# Function to check if a command exists
function Test-CommandExists {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Command
    )
    
    $exists = $null -ne (Get-Command -Name $Command -ErrorAction SilentlyContinue)
    return $exists
}

# Function to deploy an ARM template
function Deploy-ArmTemplate {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$TemplateFile,
        
        [Parameter(Mandatory = $false)]
        [string]$ParameterFile,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$OverrideParameters
    )
    
    try {
        Write-Verbose "Deploying ARM template: $TemplateFile"
        
        # Validate template file
        if (-not (Test-Path -Path $TemplateFile)) {
            throw "Template file not found: $TemplateFile"
        }
        
        # Create the resource group if it doesn't exist
        $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
        if ($null -eq $resourceGroup) {
            Write-Verbose "Creating resource group: $ResourceGroupName in location: $Location"
            New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force | Out-Null
        }
        
        # Prepare deployment parameters
        $deploymentParams = @{
            ResourceGroupName = $ResourceGroupName
            TemplateFile      = $TemplateFile
            Mode              = "Incremental"
            Name              = "Deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        }
        
        # Add parameter file if specified
        if ($ParameterFile -and (Test-Path -Path $ParameterFile)) {
            $deploymentParams.Add("TemplateParameterFile", $ParameterFile)
            Write-Verbose "Using parameter file: $ParameterFile"
        }
        
        # Add override parameters if provided
        if ($OverrideParameters -and $OverrideParameters.Count -gt 0) {
            Write-Verbose "Applying parameter overrides:"
            foreach ($key in $OverrideParameters.Keys) {
                Write-Verbose "  $key = $($OverrideParameters[$key])"
            }
            $deploymentParams.Add("TemplateParameterObject", $OverrideParameters)
        }
        
        # Deploy the template
        Write-Verbose "Starting ARM template deployment..."
        $deployment = New-AzResourceGroupDeployment @deploymentParams
        
        if ($deployment.ProvisioningState -ne "Succeeded") {
            throw "Deployment failed with state: $($deployment.ProvisioningState)"
        }
        
        Write-Verbose "ARM template deployment successful."
        return $deployment
    }
    catch {
        Write-Error "Error deploying ARM template: $_"
        throw
    }
}

# Function to deploy infrastructure using Terraform
function Deploy-Terraform {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        
        [Parameter(Mandatory = $false)]
        [string]$VarFile,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$Variables
    )
    
    try {
        # Verify Terraform is installed
        if (-not (Test-CommandExists -Command "terraform")) {
            throw "Terraform command not found. Please install Terraform and make sure it's in your PATH."
        }
        
        # Verify the working directory exists
        if (-not (Test-Path -Path $WorkingDirectory)) {
            throw "Terraform directory not found: $WorkingDirectory"
        }
        
        # Change to the Terraform directory
        Push-Location -Path $WorkingDirectory
        
        try {
            # Initialize Terraform
            Write-Verbose "Initializing Terraform..."
            $initOutput = terraform init -reconfigure
            if ($LASTEXITCODE -ne 0) {
                throw "Terraform initialization failed. Output: $initOutput"
            }
            
            # Prepare variable arguments
            $varArgs = @()
            
            # Add var file if specified
            if ($VarFile -and (Test-Path -Path $VarFile)) {
                Write-Verbose "Using variable file: $VarFile"
                $varArgs += "-var-file=`"$VarFile`""
            }
            
            # Create a temporary tfvars file if variables provided
            $tempVarFile = $null
            if ($Variables -and $Variables.Count -gt 0) {
                $tempVarFile = [System.IO.Path]::GetTempFileName()
                
                Write-Verbose "Creating temporary tfvars file: $tempVarFile"
                foreach ($key in $Variables.Keys) {
                    $value = $Variables[$key]
                    
                    # Format the value based on type
                    if ($value -is [string]) {
                        $formattedValue = "`"$value`""
                    }
                    elseif ($value -is [bool]) {
                        $formattedValue = $value.ToString().ToLower()
                    }
                    elseif ($value -is [array]) {
                        $formattedValue = "[" + ($value -join ", ") + "]"
                    }
                    else {
                        $formattedValue = $value
                    }
                    
                    "$key = $formattedValue" | Out-File -FilePath $tempVarFile -Append
                    Write-Verbose "  $key = $formattedValue"
                }
                
                $varArgs += "-var-file=`"$tempVarFile`""
            }
            
            # Run Terraform plan
            Write-Verbose "Running Terraform plan..."
            $planOutput = terraform plan @varArgs -out=tfplan
            if ($LASTEXITCODE -ne 0) {
                throw "Terraform plan failed. Output: $planOutput"
            }
            
            # Run Terraform apply
            Write-Verbose "Running Terraform apply..."
            $applyOutput = terraform apply -auto-approve tfplan
            if ($LASTEXITCODE -ne 0) {
                throw "Terraform apply failed. Output: $applyOutput"
            }
            
            Write-Verbose "Terraform deployment successful."
            return $true
        }
        finally {
            # Clean up temporary files
            if ($tempVarFile -and (Test-Path -Path $tempVarFile)) {
                Remove-Item -Path $tempVarFile -Force
            }
            
            # Return to original directory
            Pop-Location
        }
    }
    catch {
        Write-Error "Error deploying Terraform: $_"
        return $false
    }
}

# Function to deploy Kubernetes resources
function Deploy-KubernetesResources {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$ClusterName,
        
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$Namespace,
        
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$Variables
    )
    
    try {
        # Verify kubectl is installed
        if (-not (Test-CommandExists -Command "kubectl")) {
            throw "kubectl command not found. Please install kubectl and make sure it's in your PATH."
        }
        
        # Get AKS credentials
        Write-Verbose "Getting AKS credentials for cluster: $ClusterName"
        Import-AzAksCredential -ResourceGroupName $ResourceGroupName -Name $ClusterName -Force
        
        # Create namespace if it doesn't exist
        $namespaceExists = kubectl get namespace $Namespace --ignore-not-found -o jsonpath="{.metadata.name}" 2>$null
        if (-not $namespaceExists) {
            Write-Verbose "Creating namespace: $Namespace"
            kubectl create namespace $Namespace
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to create namespace: $Namespace"
            }
        }
        
        # Process manifest files
        if (Test-Path -Path $ManifestPath) {
            # If ManifestPath is a directory, process all yaml files
            if ((Get-Item -Path $ManifestPath) -is [System.IO.DirectoryInfo]) {
                $manifestFiles = Get-ChildItem -Path $ManifestPath -Filter "*.yaml" -Recurse
                
                foreach ($file in $manifestFiles) {
                    $manifestContent = Get-Content -Path $file.FullName -Raw
                    
                    # Replace variables in manifest
                    if ($Variables -and $Variables.Count -gt 0) {
                        foreach ($key in $Variables.Keys) {
                            $placeholder = "{{$key}}"
                            $manifestContent = $manifestContent -replace $placeholder, $Variables[$key]
                        }
                    }
                    
                    # Create a temporary file with the updated content
                    $tempFile = [System.IO.Path]::GetTempFileName() + ".yaml"
                    $manifestContent | Out-File -FilePath $tempFile -Encoding utf8
                    
                    # Apply the manifest
                    Write-Verbose "Applying Kubernetes manifest: $($file.Name)"
                    kubectl apply -f $tempFile -n $Namespace
                    if ($LASTEXITCODE -ne 0) {
                        throw "Failed to apply manifest: $($file.Name)"
                    }
                    
                    # Clean up temporary file
                    Remove-Item -Path $tempFile -Force
                }
            }
            # If ManifestPath is a file, process it directly
            else {
                $manifestContent = Get-Content -Path $ManifestPath -Raw
                
                # Replace variables in manifest
                if ($Variables -and $Variables.Count -gt 0) {
                    foreach ($key in $Variables.Keys) {
                        $placeholder = "{{$key}}"
                        $manifestContent = $manifestContent -replace $placeholder, $Variables[$key]
                    }
                }
                
                # Create a temporary file with the updated content
                $tempFile = [System.IO.Path]::GetTempFileName() + ".yaml"
                $manifestContent | Out-File -FilePath $tempFile -Encoding utf8
                
                # Apply the manifest
                Write-Verbose "Applying Kubernetes manifest: $ManifestPath"
                kubectl apply -f $tempFile -n $Namespace
                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to apply manifest: $ManifestPath"
                }
                
                # Clean up temporary file
                Remove-Item -Path $tempFile -Force
            }
        }
        else {
            throw "Manifest path not found: $ManifestPath"
        }
        
        # Verify deployment status
        Write-Verbose "Verifying deployment status in namespace: $Namespace"
        $deployments = kubectl get deployments -n $Namespace -o jsonpath="{.items[*].metadata.name}" 2>$null
        
        foreach ($deployment in $deployments.Split() | Where-Object { $_ }) {
            Write-Verbose "Waiting for deployment to be ready: $deployment"
            kubectl rollout status deployment/$deployment -n $Namespace --timeout=300s
            if ($LASTEXITCODE -ne 0) {
                throw "Deployment not ready: $deployment"
            }
        }
        
        Write-Verbose "Kubernetes resources deployed successfully."
        return $true
    }
    catch {
        Write-Error "Error deploying Kubernetes resources: $_"
        return $false
    }
}

# Function to deploy a Helm chart
function Deploy-HelmChart {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$ClusterName,
        
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $true)]
        [string]$Namespace,
        
        [Parameter(Mandatory = $true)]
        [string]$ChartPath,
        
        [Parameter(Mandatory = $true)]
        [string]$ReleaseName,
        
        [Parameter(Mandatory = $false)]
        [string]$ValuesFile,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$SetValues
    )
    
    try {
        # Verify helm is installed
        if (-not (Test-CommandExists -Command "helm")) {
            throw "helm command not found. Please install Helm and make sure it's in your PATH."
        }
        
        # Get AKS credentials
        Write-Verbose "Getting AKS credentials for cluster: $ClusterName"
        Import-AzAksCredential -ResourceGroupName $ResourceGroupName -Name $ClusterName -Force
        
        # Create namespace if it doesn't exist
        $namespaceExists = kubectl get namespace $Namespace --ignore-not-found -o jsonpath="{.metadata.name}" 2>$null
        if (-not $namespaceExists) {
            Write-Verbose "Creating namespace: $Namespace"
            kubectl create namespace $Namespace
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to create namespace: $Namespace"
            }
        }
        
        # Prepare Helm arguments
        $helmArgs = @(
            "upgrade",
            "--install",
            $ReleaseName,
            $ChartPath,
            "--namespace", $Namespace,
            "--create-namespace",
            "--atomic",
            "--timeout", "10m0s"
        )
        
        # Add values file if specified
        if ($ValuesFile -and (Test-Path -Path $ValuesFile)) {
            Write-Verbose "Using values file: $ValuesFile"
            $helmArgs += "--values"
            $helmArgs += $ValuesFile
        }
        
        # Add set values if provided
        if ($SetValues -and $SetValues.Count -gt 0) {
            Write-Verbose "Setting Helm values:"
            foreach ($key in $SetValues.Keys) {
                Write-Verbose "  $key = $($SetValues[$key])"
                $helmArgs += "--set"
                $helmArgs += "$key=$($SetValues[$key])"
            }
        }
        
        # Deploy the Helm chart
        Write-Verbose "Deploying Helm chart: $ChartPath with release name: $ReleaseName"
        $helmOutput = helm @helmArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Helm deployment failed. Output: $helmOutput"
        }
        
        Write-Verbose "Helm chart deployed successfully."
        return $true
    }
    catch {
        Write-Error "Error deploying Helm chart: $_"
        return $false
    }
}

# Function to initialize Azure Key Vault with required secrets
function Initialize-AzureKeyVault {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $false)]
        [hashtable]$Secrets
    )
    
    try {
        # Check if Key Vault exists
        $keyVault = Get-AzKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
        
        if ($null -eq $keyVault) {
            throw "Key Vault not found: $KeyVaultName in resource group: $ResourceGroupName"
        }
        
        Write-Verbose "Initializing Key Vault: $KeyVaultName"
        
        # Dot-source the initialize-keyvault.ps1 script
        . "$PSScriptRoot/initialize-keyvault.ps1"
        
        # Initialize Key Vault with secrets
        $secretsToInitialize = @{}
        
        foreach ($key in $Secrets.Keys) {
            $secretsToInitialize[$key] = $Secrets[$key]
        }
        
        # Set secrets in Key Vault
        foreach ($key in $secretsToInitialize.Keys) {
            $success = Set-KeyVaultSecret -KeyVaultName $KeyVaultName -SecretName $key -SecretValue $secretsToInitialize[$key]
            if (-not $success) {
                Write-Warning "Failed to set secret '$key' in Key Vault."
            }
        }
        
        # Generate random passwords for required secrets that don't have values
        $standardSecrets = @(
            "JwtSigningKey",
            "ApiKey",
            "CognitiveServicesKey",
            "ApplicationInsightsKey"
        )
        
        foreach ($secretName in $standardSecrets) {
            if (-not (Test-KeyVaultSecretExists -KeyVaultName $KeyVaultName -SecretName $secretName)) {
                $randomPassword = New-RandomPassword -Length 32 -IncludeSpecialCharacters $true
                $success = Set-KeyVaultSecret -KeyVaultName $KeyVaultName -SecretName $secretName -SecretValue $randomPassword
                if (-not $success) {
                    Write-Warning "Failed to set auto-generated secret '$secretName' in Key Vault."
                }
            }
        }
        
        # Verify secrets were created
        $success = $true
        foreach ($key in $Secrets.Keys) {
            $secretExists = Test-KeyVaultSecretExists -KeyVaultName $KeyVaultName -SecretName $key
            if (-not $secretExists) {
                Write-Warning "Secret not created: $key"
                $success = $false
            }
        }
        
        return $success
    }
    catch {
        Write-Error "Error initializing Azure Key Vault: $_"
        return $false
    }
}

# Function to get outputs from an ARM deployment or Terraform state
function Get-DeploymentOutputs {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [ValidateSet("ARM", "Terraform")]
        [string]$DeploymentType,
        
        [Parameter(Mandatory = $false)]
        [string]$ResourceGroupName,
        
        [Parameter(Mandatory = $false)]
        [string]$DeploymentName,
        
        [Parameter(Mandatory = $false)]
        [string]$TerraformDirectory
    )
    
    try {
        $outputs = @{}
        
        if ($DeploymentType -eq "ARM") {
            if ([string]::IsNullOrEmpty($ResourceGroupName) -or [string]::IsNullOrEmpty($DeploymentName)) {
                throw "ResourceGroupName and DeploymentName are required for ARM deployment outputs."
            }
            
            # Get ARM deployment outputs
            $deployment = Get-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -Name $DeploymentName
            
            if ($null -ne $deployment -and $null -ne $deployment.Outputs) {
                foreach ($key in $deployment.Outputs.Keys) {
                    $outputs[$key] = $deployment.Outputs[$key].Value
                }
            }
        }
        elseif ($DeploymentType -eq "Terraform") {
            if ([string]::IsNullOrEmpty($TerraformDirectory)) {
                throw "TerraformDirectory is required for Terraform deployment outputs."
            }
            
            # Verify Terraform is installed
            if (-not (Test-CommandExists -Command "terraform")) {
                throw "Terraform command not found. Please install Terraform and make sure it's in your PATH."
            }
            
            # Change to the Terraform directory
            Push-Location -Path $TerraformDirectory
            
            try {
                # Get Terraform outputs as JSON
                $terraformOutputJson = terraform output -json
                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to get Terraform outputs."
                }
                
                # Parse JSON
                $terraformOutputs = $terraformOutputJson | ConvertFrom-Json
                
                # Extract outputs
                foreach ($key in $terraformOutputs.PSObject.Properties.Name) {
                    $outputs[$key] = $terraformOutputs.$key.value
                }
            }
            finally {
                # Return to original directory
                Pop-Location
            }
        }
        else {
            throw "Unsupported deployment type: $DeploymentType"
        }
        
        return $outputs
    }
    catch {
        Write-Error "Error getting deployment outputs: $_"
        return @{}
    }
}

# Main script execution
try {
    Write-Host "Starting deployment of VAT Filing Pricing Tool infrastructure..." -ForegroundColor Green
    Write-Host "Deployment Type: $DeploymentType, Environment: $Environment, Region: $Location" -ForegroundColor Green
    Write-Host "----------------------------------------------------------------------" -ForegroundColor Green
    
    # Import required modules
    $requiredModules = @(
        @{ Name = "Az.Resources"; MinVersion = "6.0.0" },
        @{ Name = "Az.KeyVault"; MinVersion = "4.0.0" },
        @{ Name = "Az.Sql"; MinVersion = "3.0.0" },
        @{ Name = "Az.Storage"; MinVersion = "5.0.0" },
        @{ Name = "Az.CosmosDB"; MinVersion = "1.0.0" },
        @{ Name = "Az.Kubernetes"; MinVersion = "2.0.0" }
    )
    
    foreach ($module in $requiredModules) {
        Write-Verbose "Importing module: $($module.Name)"
        
        # Check if module is already imported
        $importedModule = Get-Module -Name $module.Name -ErrorAction SilentlyContinue
        
        if ($null -eq $importedModule) {
            # Check if module is installed
            $installedModule = Get-Module -Name $module.Name -ListAvailable -ErrorAction SilentlyContinue
            
            if ($null -eq $installedModule -or 
                ($module.MinVersion -and ($installedModule.Version -lt [version]$module.MinVersion))) {
                Write-Warning "Module $($module.Name) (minimum version $($module.MinVersion)) not found or version too low."
                Write-Warning "Please install it with: Install-Module -Name $($module.Name) -MinimumVersion $($module.MinVersion) -Force"
                throw "Required module not installed: $($module.Name)"
            }
        }
        
        # Import the module
        Import-Module -Name $module.Name -MinimumVersion $module.MinVersion -Force -ErrorAction Stop
    }
    
    # Validate input parameters
    if ($DeploymentType -eq "ARM") {
        if (-not (Test-Path -Path $TemplateFile)) {
            throw "ARM template file not found: $TemplateFile"
        }
        
        if ($ParameterFile -and -not (Test-Path -Path $ParameterFile)) {
            throw "ARM parameter file not found: $ParameterFile"
        }
    }
    elseif ($DeploymentType -eq "Terraform") {
        if (-not (Test-Path -Path $TerraformDirectory)) {
            throw "Terraform directory not found: $TerraformDirectory"
        }
        
        if ($TerraformVarFile -and -not (Test-Path -Path $TerraformVarFile)) {
            Write-Warning "Terraform variable file not found: $TerraformVarFile"
        }
    }
    
    if ($DeployKubernetes) {
        if (-not (Test-Path -Path $KubernetesDirectory)) {
            throw "Kubernetes directory not found: $KubernetesDirectory"
        }
        
        if ($HelmDirectory -and -not (Test-Path -Path $HelmDirectory)) {
            throw "Helm directory not found: $HelmDirectory"
        }
    }
    
    # Get Azure context
    $context = Get-AzContext
    if (-not $context) {
        Write-Verbose "Not connected to Azure. Connecting..."
        Connect-AzAccount
        $context = Get-AzContext
    }
    
    Write-Verbose "Using Azure context: $($context.Name)"
    Write-Verbose "Subscription: $($context.Subscription.Name) ($($context.Subscription.Id))"
    Write-Verbose "Tenant: $($context.Tenant.Id)"
    
    # Create resource group if it doesn't exist
    $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
    if ($null -eq $resourceGroup) {
        Write-Verbose "Creating resource group: $ResourceGroupName in location: $Location"
        $resourceGroup = New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force
    }
    
    # Deploy infrastructure
    $deploymentOutputs = @{}
    
    if ($DeploymentType -eq "ARM") {
        # Prepare parameters for ARM template
        $templateParameters = @{
            "environment"       = $Environment
            "location"          = $Location
            "secondaryLocation" = $SecondaryLocation
            "appName"           = $AppName
            "aksNodeCount"      = $AksNodeCount
            "aksNodeSize"       = $AksNodeSize
        }
        
        # Add SQL admin credentials if provided
        if ($SqlAdminUsername) {
            $templateParameters["sqlAdminUsername"] = $SqlAdminUsername
        }
        
        if ($SqlAdminPassword) {
            $templateParameters["sqlAdminPassword"] = (ConvertFrom-SecureString -SecureString $SqlAdminPassword -AsPlainText)
        }
        
        if ($AcrName) {
            $templateParameters["acrName"] = $AcrName
        }
        
        # Deploy ARM template
        $armDeployment = Deploy-ArmTemplate -ResourceGroupName $ResourceGroupName -TemplateFile $TemplateFile -ParameterFile $ParameterFile -OverrideParameters $templateParameters
        
        # Get deployment outputs
        $deploymentOutputs = Get-DeploymentOutputs -DeploymentType ARM -ResourceGroupName $ResourceGroupName -DeploymentName $armDeployment.DeploymentName
    }
    elseif ($DeploymentType -eq "Terraform") {
        # Prepare variables for Terraform
        $terraformVariables = @{
            "environment"       = $Environment
            "location"          = $Location
            "secondary_location" = $SecondaryLocation
            "app_name"          = $AppName
            "resource_group_name" = $ResourceGroupName
            "aks_node_count"    = $AksNodeCount
            "aks_node_size"     = $AksNodeSize
        }
        
        # Add SQL admin credentials if provided
        if ($SqlAdminUsername) {
            $terraformVariables["sql_admin_username"] = $SqlAdminUsername
        }
        
        if ($SqlAdminPassword) {
            $terraformVariables["sql_admin_password"] = (ConvertFrom-SecureString -SecureString $SqlAdminPassword -AsPlainText)
        }
        
        if ($AcrName) {
            $terraformVariables["acr_name"] = $AcrName
        }
        
        # Deploy Terraform
        $terraformSuccess = Deploy-Terraform -WorkingDirectory $TerraformDirectory -VarFile $TerraformVarFile -Variables $terraformVariables
        
        if (-not $terraformSuccess) {
            throw "Terraform deployment failed."
        }
        
        # Get Terraform outputs
        $deploymentOutputs = Get-DeploymentOutputs -DeploymentType Terraform -TerraformDirectory $TerraformDirectory
    }
    
    # Initialize Key Vault with required secrets
    if ($deploymentOutputs.ContainsKey("keyVaultName")) {
        $keyVaultName = $deploymentOutputs["keyVaultName"]
        
        # Prepare secrets for Key Vault
        $secrets = @{}
        
        # Add connection strings from deployment outputs
        if ($deploymentOutputs.ContainsKey("sqlConnectionString")) {
            $secrets["SqlConnectionString"] = $deploymentOutputs["sqlConnectionString"]
        }
        
        if ($deploymentOutputs.ContainsKey("cosmosDbConnectionString")) {
            $secrets["CosmosDbConnectionString"] = $deploymentOutputs["cosmosDbConnectionString"]
        }
        
        if ($deploymentOutputs.ContainsKey("storageConnectionString")) {
            $secrets["StorageConnectionString"] = $deploymentOutputs["storageConnectionString"]
        }
        
        if ($deploymentOutputs.ContainsKey("redisConnectionString")) {
            $secrets["RedisConnectionString"] = $deploymentOutputs["redisConnectionString"]
        }
        
        # Add SQL credentials
        if ($SqlAdminUsername) {
            $secrets["SqlAdminUsername"] = $SqlAdminUsername
        }
        
        if ($SqlAdminPassword) {
            $secrets["SqlAdminPassword"] = (ConvertFrom-SecureString -SecureString $SqlAdminPassword -AsPlainText)
        }
        
        # Add other standard secrets
        $secrets["AzureAdClientId"] = ""
        $secrets["AzureAdClientSecret"] = ""
        $secrets["AzureAdTenantId"] = $context.Tenant.Id
        $secrets["JwtSigningKey"] = ""
        $secrets["ApiKey"] = ""
        $secrets["CognitiveServicesKey"] = ""
        $secrets["ApplicationInsightsKey"] = ""
        
        # Initialize Key Vault
        $keyVaultSuccess = Initialize-AzureKeyVault -KeyVaultName $keyVaultName -ResourceGroupName $ResourceGroupName -Secrets $secrets
        
        if (-not $keyVaultSuccess) {
            Write-Warning "Key Vault initialization was not completely successful. Some secrets may not be properly configured."
        }
    }
    else {
        Write-Warning "Key Vault name not found in deployment outputs. Skipping Key Vault initialization."
    }
    
    # Deploy Kubernetes resources if requested
    if ($DeployKubernetes) {
        if ($deploymentOutputs.ContainsKey("aksClusterName")) {
            $aksClusterName = $deploymentOutputs["aksClusterName"]
            $aksResourceGroup = $ResourceGroupName
            
            if ($deploymentOutputs.ContainsKey("aksResourceGroup")) {
                $aksResourceGroup = $deploymentOutputs["aksResourceGroup"]
            }
            
            # Define Kubernetes namespace
            $namespace = "$AppName-$Environment"
            
            # Prepare variables for Kubernetes manifests
            $k8sVariables = @{
                "NAMESPACE"     = $namespace
                "ENVIRONMENT"   = $Environment
                "APP_NAME"      = $AppName
                "BUILD_VERSION" = $BuildVersion
            }
            
            # Add connection strings and other outputs
            foreach ($key in $deploymentOutputs.Keys) {
                $k8sVariables[$key.ToUpper()] = $deploymentOutputs[$key]
            }
            
            # Get AKS credentials
            Write-Verbose "Getting AKS credentials for cluster: $aksClusterName"
            Import-AzAksCredential -ResourceGroupName $aksResourceGroup -Name $aksClusterName -Force
            
            # Create namespace if it doesn't exist
            $namespaceExists = kubectl get namespace $namespace --ignore-not-found -o jsonpath="{.metadata.name}" 2>$null
            if (-not $namespaceExists) {
                Write-Verbose "Creating namespace: $namespace"
                kubectl create namespace $namespace
            }
            
            # Deploy Kubernetes manifests
            $k8sSuccess = Deploy-KubernetesResources -ClusterName $aksClusterName -ResourceGroupName $aksResourceGroup -Namespace $namespace -ManifestPath $KubernetesDirectory -Variables $k8sVariables
            
            if (-not $k8sSuccess) {
                Write-Warning "Kubernetes resources deployment was not completely successful."
            }
            
            # Deploy Helm chart if directory exists
            if (Test-Path -Path $HelmDirectory) {
                $helmReleaseName = "$AppName"
                $helmValuesFile = "$HelmDirectory/values-$Environment.yaml"
                
                # Prepare Helm values
                $helmValues = @{
                    "environment"   = $Environment
                    "image.tag"     = $BuildVersion
                    "replicaCount"  = if ($Environment -eq "prod") { 3 } else { 1 }
                }
                
                # Add registry information if ACR is used
                if ($deploymentOutputs.ContainsKey("acrLoginServer")) {
                    $helmValues["image.repository"] = "$($deploymentOutputs["acrLoginServer"])/$AppName"
                }
                
                # Deploy Helm chart
                $helmSuccess = Deploy-HelmChart -ClusterName $aksClusterName -ResourceGroupName $aksResourceGroup -Namespace $namespace -ChartPath $HelmDirectory -ReleaseName $helmReleaseName -ValuesFile $helmValuesFile -SetValues $helmValues
                
                if (-not $helmSuccess) {
                    Write-Warning "Helm chart deployment was not completely successful."
                }
                
                # Wait for deployments to be ready
                Write-Verbose "Waiting for deployments to be ready in namespace: $namespace"
                kubectl get deployments -n $namespace -o jsonpath="{.items[*].metadata.name}" | 
                ForEach-Object { 
                    $deployment = $_
                    Write-Verbose "Checking deployment: $deployment"
                    kubectl rollout status deployment/$deployment -n $namespace --timeout=300s
                }
                
                # Display service endpoints
                Write-Verbose "Getting service endpoints in namespace: $namespace"
                $services = kubectl get services -n $namespace -o json | ConvertFrom-Json
                
                Write-Host "Service Endpoints:" -ForegroundColor Green
                foreach ($service in $services.items) {
                    $serviceType = $service.spec.type
                    $serviceName = $service.metadata.name
                    
                    if ($serviceType -eq "LoadBalancer") {
                        $ip = $service.status.loadBalancer.ingress.ip
                        $port = $service.spec.ports[0].port
                        
                        if ($ip) {
                            Write-Host "  $serviceName : http://$ip`:$port" -ForegroundColor Cyan
                        }
                        else {
                            Write-Host "  $serviceName : <pending>" -ForegroundColor Yellow
                        }
                    }
                    elseif ($serviceType -eq "ClusterIP") {
                        Write-Host "  $serviceName : (internal) $($service.spec.clusterIP)" -ForegroundColor Gray
                    }
                }
            }
        }
        else {
            Write-Warning "AKS cluster name not found in deployment outputs. Skipping Kubernetes deployment."
        }
    }
    
    # Output deployment summary
    Write-Host ""
    Write-Host "Deployment Summary" -ForegroundColor Green
    Write-Host "-----------------" -ForegroundColor Green
    Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Cyan
    
    if ($deploymentOutputs.Count -gt 0) {
        Write-Host "Deployed Resources:" -ForegroundColor Cyan
        foreach ($key in ($deploymentOutputs.Keys | Sort-Object)) {
            # Skip connection strings and other sensitive data
            if ($key -like "*ConnectionString*" -or $key -like "*Password*" -or $key -like "*Secret*" -or $key -like "*Key*") {
                Write-Host "  $key : <redacted>" -ForegroundColor Gray
            }
            else {
                Write-Host "  $key : $($deploymentOutputs[$key])" -ForegroundColor White
            }
        }
    }
    
    Write-Host ""
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Verify all resources are deployed and configured correctly" -ForegroundColor Yellow
    Write-Host "2. Configure application settings in Azure App Service or update Kubernetes ConfigMaps" -ForegroundColor Yellow
    Write-Host "3. Deploy the application code to the infrastructure" -ForegroundColor Yellow
    Write-Host ""
}
catch {
    Write-Host "Deployment failed with error:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    
    # Attempt to provide remediation steps based on the error
    Write-Host ""
    Write-Host "Possible remediation steps:" -ForegroundColor Yellow
    
    if ($_.Exception.Message -like "*NotFound*") {
        Write-Host "- Verify that all resource names and paths are correct" -ForegroundColor Yellow
        Write-Host "- Check that you have the necessary permissions in your Azure subscription" -ForegroundColor Yellow
    }
    elseif ($_.Exception.Message -like "*authorization*" -or $_.Exception.Message -like "*permission*") {
        Write-Host "- Verify that you have the necessary permissions in your Azure subscription" -ForegroundColor Yellow
        Write-Host "- Try running the script with elevated permissions" -ForegroundColor Yellow
    }
    elseif ($_.Exception.Message -like "*quota*") {
        Write-Host "- You may have reached a resource quota limit in your subscription" -ForegroundColor Yellow
        Write-Host "- Request a quota increase or try a different region" -ForegroundColor Yellow
    }
    elseif ($_.Exception.Message -like "*command*not*found*") {
        Write-Host "- Ensure all required tools (Terraform, kubectl, helm) are installed and in your PATH" -ForegroundColor Yellow
    }
    else {
        Write-Host "- Check the error message for specific details on what went wrong" -ForegroundColor Yellow
        Write-Host "- Verify that all parameters are correct and that resources exist" -ForegroundColor Yellow
        Write-Host "- Ensure you have the latest Az PowerShell modules installed" -ForegroundColor Yellow
    }
    
    exit 1
}