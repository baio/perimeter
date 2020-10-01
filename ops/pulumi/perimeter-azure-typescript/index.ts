import * as pulumi from '@pulumi/pulumi';
import * as azure from '@pulumi/azure';

// Create an Azure Resource Group
const resourceGroup = new azure.core.ResourceGroup('prr');

// Create an Azure resource (Storage Account)
const storageAccount = new azure.storage.Account('prr7storage', {
    // The location for the storage account will be derived automatically from the resource group.
    resourceGroupName: resourceGroup.name,
    accountTier: 'Standard',
    accountReplicationType: 'LRS',
    allowBlobPublicAccess: true,
});

// Export the connection string for the storage account
export const storageConnectionString = storageAccount.primaryConnectionString;

// psql
const psqlServer = new azure.postgresql.Server('prr7psql', {
    resourceGroupName: resourceGroup.name,
    skuName: 'B_Gen5_1',
    location: resourceGroup.location,
    version: '11',
    administratorLogin: 'maxp',
    administratorLoginPassword: 'pa$$w0rd',
    publicNetworkAccessEnabled: true,    
    sslEnforcementEnabled: false,
});

export const psqlUrnString = psqlServer.urn;

const psqlDb = new azure.postgresql.Database('prr7db', {
    resourceGroupName: resourceGroup.name,
    serverName: psqlServer.name,
    charset: 'UTF8',
    collation: 'English_United States.1252',
});

// mongo
const mongoDatabase = new azure.cosmosdb.MongoDatabase("prr7db", {
    resourceGroupName: resourceGroup.name,
    accountName: storageAccount.name,
    throughput: 400,
});

export const mongoUrnString = mongoDatabase.urn;