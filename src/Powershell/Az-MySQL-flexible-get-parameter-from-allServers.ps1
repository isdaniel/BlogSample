param (
    [Parameter(Mandatory=$true)]
    [string]$parameterName,
    [Parameter(Mandatory=$true)]
    [string]$parameterValue
)

$jsonOutput = az mysql flexible-server list | ConvertFrom-Json
#$parametername = 'innodb_adaptive_hash_index'

foreach ($item in $jsonOutput) {

    $name = $item.name
    $resourceGroup = $item.resourceGroup

    $parameterInfo = az mysql flexible-server parameter show --resource-group $resourceGroup --server-name $name --name  $parameterName  | ConvertFrom-Json
    
    if ($parameterInfo.value -eq $parameterValue) {
        Write-Output "Name: $name, Resource Group: $resourceGroup, $parameterName is $parameterValue"
        
        ## we can modify parameter to another value via script.
        ## az mysql flexible-server parameter set --resource-group $resourceGroup --server-name $name --name $parameterValue --value 'your expected value'
    }
}
