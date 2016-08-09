import time

from azure.storage import AccessPolicy
from azure.storage.blob import BlockBlobService, ContentSettings, ContainerPermissions

from datetime import datetime, timedelta

# The name of the new Shared Access policy
policy_name = 'readandlistonly'
# The Storage Account Name
storage_account_name = 'mystore'
storage_account_key = 'mykey'
storage_container_name = 'mycontainer'
example_file_path = '..\\sampledata\\sample.log'
policy_name = 'mysaspolicy'

# Create the blob service, using the name and key for your Azure Storage account
blob_service = BlockBlobService(storage_account_name, storage_account_key)

# Create the container, if it does not already exist
blob_service.create_container(storage_container_name)

# Upload an example file to the container
blob_service.create_blob_from_path(
    storage_container_name,
    'sample.log',
    example_file_path,
)

# Create a new policy that expires after a week
access_policy = AccessPolicy(permission=ContainerPermissions.READ + ContainerPermissions.LIST, expiry=datetime.utcnow() + timedelta(weeks=1))



# Get the existing identifiers (policies) for the container
identifiers = blob_service.get_container_acl(storage_container_name)
# And add the new one ot the list
identifiers[policy_name] = access_policy

# Set the container to the updated list of identifiers (policies)
blob_service.set_container_acl(
    storage_container_name,
    identifiers,
)

# Wait 30 seconds for acl to propagate
time.sleep(30)

# Generate a new Shared Access Signature token using the policy (by name)
sas_token = blob_service.generate_container_shared_access_signature(
    storage_container_name,
    id=policy_name,
)

# Print out the new token
print(sas_token)