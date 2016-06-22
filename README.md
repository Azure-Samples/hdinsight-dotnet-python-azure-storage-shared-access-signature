---
services: hdinsight
platforms: dotnet,python
author: blackmist
---

# hdinsight-dotnet-python-azure-storage-shared-access-signature
How to restrict access to Azure blob storage from HDInsight by using shared access signatures. This sample spans HDInsight and Azure Storage, and samples are provided for dotnet and python.

## Create a Shared Access Signature

You can use either the __SASExample__ solution (C#) or __SASToken.py__ (Python) to retrieve a Shared Access Signature (SAS) for an existing Azure Blob Storage account.

### Using SASExample (C\#)

1. Open the project in Visual Studio. It's contained in the `CSharp` directory of this repository.

2. Right click on the project in Solution Explorer, then select properties.

3. In properties, select __Settings__.

4. In settings, populate the following entries:

    * StorageConnectionString: The connection string for the storage account that you want to create a stored policy and SAS for. The format should be `DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=mykey` where `myaccount` is the name of your storage account and `mykey` is the key for the storage account.
    
    * ContainerName: The container in the storage account that you want to restrict access to.
    
    * SASPolicyName: The name to use for the stored policy that will be created.
    
    * FileToUpload: The path to a file that will be uploaded to the container. There's a `sample.log` file in the `sampledata` folder of this project that can be used.
    
4. Run the project. It will open a console window and display the SAS token created using the policy. This can be used to provide read and list access to the container. Save the token for later use.

### Using SASToken.py (Python)

1. Open the `SASToken.py` file (in the `Python` directory of this repository,) and change the following values:

    * policy\_name: The name to use for the stored policy that will be created.
    
    * storage\_account\_name: The name of your storage account.
    
    * storage\_account\_key: The key for the storage account.
    
    * storage\_container\_name: The container in the storage account that you want to restrict access to.
    
    * example\_file\_path: The path to a file that will be uploaded to the container.

2. Run the script. It will display the SAS token created using the policy. This can be used to provide read and list access to the container. Save the token for later use.

## Create an HDInsight cluster that uses the token

1. Open the `HDInsightSAS.ps1` from the `CreateCluster` directory of this repository.

2. Replace the following values:

	* $clusterName - set this to the name you want to use for the new HDInsight cluster. It must be a unique name.
	* $osType - set this to 'Linux' or 'Windows' to set the OS of the HDInsight cluster.
	* $resourceGroupName - set this to the name of the resource group that will contain the cluster.
	* $location - set this to the name of the Azure region that the cluster will be created in.
	* $defaultStorageAccountName - set this to the name of a storage account. This is where the default, read/write access storage for the cluster will be created. This should be a different storage account than the one used for the SAS token.
	* $SASStorageAccountName - set this to the name of the storage account that you used when generating the SAS token.
	* $SASContainerName - set this to the name of the container that you used when generating the SAS token.
	* $SASToken - set this to the SAS token that you generated earlier

	Save the file after you have made changes.

2. Open a PowerShell prompt and authenticate to your Azure subscription:

		Add-AzureRmAccount

3. Run the script from the PowerShell Prompt.

		.\HDinsightSAS.ps1
	
	It will take around 15 minutes to complete the cluster creation process.

## Update an existing Linux-based cluster to use the token

If you have an existing Linux-based HDInsightr cluster, you can update it to use the SAS secured storage.



1. Open the Ambari web UI for your cluster. The address for this page is https://YOURCLUSTERNAME.azurehdinsight.net. When prompted, authenticate to the cluster using the admin name (admin,) and password you used when creating the cluster.

2 From the left side of the Ambari web UI, select HDFS and then select the Configs tab in the middle of the page.

3. Select the Advanced tab, and then scroll until you find the Custom core-site section.

4 Expand the Custom core-site section, then scroll to the end and select the Add property... link. Use the following values for the Key and Value fields:

		Key: fs.azure.sas.CONTAINERNAME.STORAGEACCOUNTNAME.blob.core.windows.net
		Value: The SAS returned by the C# or Python application you ran previously

    Replace CONTAINERNAME with the container name you used with the C# or SAS application. Replace STORAGEACCOUNTNAME with the storage account name you used.

5. Click the Add button to save this key and value, then click the Save button to save the configuration changes. When prompted, add a description of the change ("adding SAS storage access" for example,) and then click Save.

    Click OK when the changes have been completed.
    
	This saves the configuration changes, but you must restart several services before the change takes effect.

6. In the Ambari web UI, select HDFS from the list on the left, and then select Restart All from the Service Actions drop down list on the right. When prompted, select Turn on maintenance mode and then select __Conform Restart All".

    Repeat this process for the MapReduce2 and YARN entries from the list on the left of the page.

7. Once these have restarted, select each one and disable maintenance mode from the Service Actions drop down.

##Test restricted access

To verify that you have restricted access, use the following methods:

* For __Windows-based__ HDInsight clusters, use Remote Desktop to connect to the cluster. See [Connecto to HDInsight using RDP](hdinsight-administer-use-management-portal.md#connect-to-clusters-using-rdp) for more information.

    Once connected, use the __Hadoop Command Line__ icon on the desktop to open a command prompt.

* For __Linux-based__ HDInsight clusters, use SSH to connect to the cluster. See one of the following for information on using SSH with Linux-based clusters:

    * [Use SSH with Linux-based Hadoop on HDInsight from Linux, OS X, and Unix](hdinsight-hadoop-linux-use-ssh-unix.md)
    * [Use SSH with Linux-based Hadoop on HDInsight from Windows](hdinsight-hadoop-linux-use-ssh-windows.md)
    
Once connected to the cluster, use the following steps to verify that you can only read and list items on the SAS storage account:

1. From the prompt, type the following. Replace __SASCONTAINER__ with the name of the container created for the SAS storage account. Replace __SASACCOUNTNAME__ with the name of the storage account used for the SAS:

        hdfs dfs -ls wasb://SASCONTAINER@SASACCOUNTNAME.blob.core.windows.net/
    
    This will list the contents of the container, which should include the file that was uploaded when the container and SAS was created.
    
2. Use the following to verify that you can read the contents of the file. Replace the __SASCONTAINER__ and __SASACCOUNTNAME__ as in the previous step. Replace __FILENAME__ with the name of the file displayed in the previous command:

        hdfs dfs -text wasb://SASCONTAINER@SASACCOUNTNAME.blob.core.windows.net/FILENAME
        
    This will list the contents of the file.
    
3. Use the following to download the file to the local file system:

        hdfs dfs -get wasb://SASCONTAINER@SASACCOUNTNAME.blob.core.windows.net/FILENAME testfile.txt
    
    This will download the file to a local file named __testfile.txt__.

4. Use the following to upload the local file to a new file named __testupload.txt__ on the SAS storage:

        hdfs dfs -put testfile.txt wasb://SASCONTAINER@SASACCOUNTNAME.blob.core.windows.net/testupload.txt
    
    You will receive a message similar to the following:
    
        put: java.io.IOException
        
    This error occurs because the storage location is read+list only. Use the following to put the data on the default storage for the cluster, which is writable:
    
        hdfs dfs -put testfile.txt wasb:///testupload.txt
        
    This time, the operation should complete successfully.

## Project code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.