using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace sas
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference(Properties.Settings.Default.ContainerName);
            //Create the container if it does not exist.
            container.CreateIfNotExists();

            //Upload a file to the container
            UploadFile(blobClient, container, Properties.Settings.Default.FileToUpload);

            //Uncomment the following lines to clear any existing access policies on container.
            //BlobContainerPermissions perms = container.GetPermissions();
            //perms.SharedAccessPolicies.Clear();
            //container.SetPermissions(perms);

            //Create a new access policy on the container, which may be optionally used to provide constraints for 
            //shared access signatures on the container and the blob.
            string sharedAccessPolicyName = Properties.Settings.Default.SASPolicyName;
            CreateSharedAccessPolicy(blobClient, container, sharedAccessPolicyName);

            //Generate a SAS token for the container, using a stored access policy to set constraints on the SAS.
            Console.WriteLine("Container SAS token using stored access policy: {0}\r\n", GetContainerSasTokenWithPolicy(container, sharedAccessPolicyName));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        /// <summary>
        /// Upload a sample file to the container
        /// </summary>
        /// <param name="blobClient">The blob client used to connect to storage</param>
        /// <param name="container">The container that the file will be uploaded to</param>
        /// <param name="fileToUpload">The file to upload</param>
        static void UploadFile(CloudBlobClient blobClient, CloudBlobContainer container, string fileToUpload)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("sample.log");

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(fileToUpload))
            {
                blockBlob.UploadFromStream(fileStream);
            }
        }

        /// <summary>
        /// Creates a new Shared Access Policy for the container
        /// </summary>
        /// <param name="blobClient">The blob client to use for this operation</param>
        /// <param name="container">The container to create the policy for</param>
        /// <param name="policyName">The name of the policy</param>
        static void CreateSharedAccessPolicy(CloudBlobClient blobClient, CloudBlobContainer container,
    string policyName)
        {
            //Create a new shared access policy and define its constraints.
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                // What is the expiration date of this policy?
                SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                // What permissions does this policy grant?
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
            };

            //Get the container's existing permissions.
            BlobContainerPermissions permissions = container.GetPermissions();

            //Add the new policy to the container's permissions, and set the container's permissions.
            permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
            container.SetPermissions(permissions);
        }

        /// <summary>
        /// Gets the SharedAccessSignature token for the container, using the specified policy
        /// </summary>
        /// <param name="container">The container to get a token for</param>
        /// <param name="policyName">The policy to use when getting the token</param>
        /// <returns></returns>
        static string GetContainerSasTokenWithPolicy(CloudBlobContainer container, string policyName)
        {
            //Generate the shared access signature on the container. In this case, all of the constraints for the 
            //shared access signature are specified on the stored access policy.
            string sasContainerToken = container.GetSharedAccessSignature(null, policyName);

            //Return the SAS token.
            return sasContainerToken;
        }
    }
}
