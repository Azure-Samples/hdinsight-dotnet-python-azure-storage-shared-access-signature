from azure.storage import AccessPolicy, SharedAccessPolicy, SignedIdentifier, SignedIdentifiers
from azure.storage.blob import BlobService, ContainerSharedAccessPermissions

# The name of the new Shared Access policy
policy_name = 'readandlistonly'
# The Storage Account Name
storage_account_name = 'myaccountname'
storage_account_key = 'mykey'
storage_container_name = 'mycontainer'
example_file_path = '..\\sampledata\\sample.log'

# Create the blob service, using the name and key for your Azure Storage account
blob_service = BlobService(storage_account_name, storage_account_key)

# Create the container, if it does not already exist
blob_service.create_container(storage_container_name)

# Upload an example file to the container
blob_service.put_block_blob_from_path(
    storage_container_name,
    'sample.log',
    example_file_path,
)

# Create a new signed identifier (policy)
si = SignedIdentifier()
# Set the name
si.id = policy_name
# Set the expiration date
si.access_policy.expiry = '2016-01-01'
# Set the permissions. Read and List in this example
si.access_policy.permission = ContainerSharedAccessPermissions.READ + ContainerSharedAccessPermissions.LIST

# Get the existing signed identifiers (policies) for the container
identifiers = blob_service.get_container_acl(storage_container_name)
# And append the new one ot the list
identifiers.signed_identifiers.append(si)

# Set the container to the updated list of signed identifiers (policies)
blob_service.set_container_acl(
    container_name=storage_container_name,
    signed_identifiers=identifiers,
)

# Generate a new Shared Access Signature token using the 
sas_token = blob_service.generate_shared_access_signature(
    container_name=storage_container_name,
    shared_access_policy=SharedAccessPolicy(signed_identifier=policy_name),
)

# Print out the new token
print sas_token