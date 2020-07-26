namespace Common.Test.Utils

open Microsoft.Azure.Storage


module AzureBlob =
    
    open Microsoft.Azure.Storage.Blob
    let removeBlobContainer connectionStr blobContainerName =
        let storageAccount = CloudStorageAccount.Parse(connectionStr);
        let blob = storageAccount.CreateCloudBlobClient()
        let containerRef = blob.GetContainerReference blobContainerName
        containerRef.Delete()

    open Microsoft.Azure.Cosmos.Table
    let removeTable connectionStr tableName =
        let storageAccount = CloudStorageAccount.Parse(connectionStr);
        let table = storageAccount.CreateCloudTableClient()
        let tableRef = table.GetTableReference tableName
        tableRef.Delete()
