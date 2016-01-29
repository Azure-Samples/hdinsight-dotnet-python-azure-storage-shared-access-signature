# Replace 'mycluster' with the name of the cluster to be created
$clusterName = 'mycluster'
# Valid values are 'Linux' and 'Windows'
$osType = 'Linux'
# Replace 'myresourcegroup' with the name of the group to be created
$resourceGroupName = 'myresourcegroup'
# Replace with the Azure data center you want to the cluster to live in
$location = 'North Europe'
# Replace with the name of the default storage account to be created
$defaultStorageAccountName = 'mystorageaccount'
# Replace with the name of the SAS container created earlier
$SASContainerName = 'sascontainer'
# Replace with the name of the SAS storage account created earlier
$SASStorageAccountName = 'sasaccount'
# Replace with the SAS token generated earlier
$SASToken = 'sastoken'
# Set the number of worker nodes in the cluster
$clusterSizeInNodes = 2
    
# Create the resource group
New-AzureRmResourceGroup `
    -Name $resourceGroupName `
    -Location $location

# Preapre default storage account and container
New-AzureRmStorageAccount `
    -ResourceGroupName $resourceGroupName `
    -Name $defaultStorageAccountName `
    -Location $location `
    -Type Standard_GRS
$defaultStorageAccountKey = Get-AzureRmStorageAccountKey `
    -ResourceGroupName $resourceGroupName `
    -Name $defaultStorageAccountName `
    |  %{ $_.Key1 }
$defaultStorageContext = New-AzureStorageContext `
    -StorageAccountName $defaultStorageAccountName `
    -StorageAccountKey $defaultStorageAccountKey
New-AzureStorageContainer `
    -Name $clusterName `
    -Context $defaultStorageContext #use the cluster name as the container name
    
# Create the configuration for the cluster
$config = New-AzureRmHDInsightClusterConfig
$config.DefaultStorageAccountName = "$defaultStorageAccountName.blob.core.windows.net"
$config.DefaultStorageAccountKey = $defaultStorageAccountKey 
$config = Add-AzureRMHDInsightConfigValues `
    -Config $config `
    -Core @{
        "fs.azure.sas.$SASContainerName.$SASStorageAccountName.blob.core.windows.net"=$SASToken;
    }

# Prompt for the admin/http credentials for the cluster
$httpCredential = Get-Credential `
    -Message "Enter an HTTP account name and password for the cluster:"

# Depending on $osType, we do things slightly differently
if($osType -eq 'linux') {
    # For Linux clusters, we have to set an SSH user account.
    # SSH is used to remotely access the cluster using this account.
    $sshCredential = Get-Credential `
        -Message "Enter an SSH account name and password for the cluster:"
    New-AzureRmHDInsightCluster `
        -Config $config `
        -ResourceGroupName $resourceGroupName `
        -ClusterName $clusterName `
        -Location $location `
        -ClusterSizeInNodes $clusterSizeInNodes `
        -ClusterType Hadoop `
        -OSType Linux `
        -HttpCredential $httpCredential `
        -SshCredential $sshCredential
} else {
    # For Windows, just create the cluster
    New-AzureRmHDInsightCluster `
        -Config $config `
        -ResourceGroupName $resourceGroupName `
        -ClusterName $clusterName `
        -Location $location `
        -ClusterSizeInNodes $clusterSizeInNodes `
        -ClusterType Hadoop `
        -OSType Windows `
        -HttpCredential $httpCredential
}
