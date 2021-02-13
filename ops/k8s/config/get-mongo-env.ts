import * as pulumi from '@pulumi/pulumi';

export const getMongoConfig = (pulumiConfig: pulumi.Config) => ({
    storageCapacity: pulumiConfig.requireNumber('mongo_storageSize'),
    hostPaths: pulumiConfig.require('mongo_dataPath'),
});

export const getMongoEnv = (pulumiConfig: pulumi.Config) => ({
    MONGO_INITDB_ROOT_USERNAME: pulumiConfig.require('mongo_MONGODB_ROOT_USER'),
    MONGO_INITDB_ROOT_PASSWORD: pulumiConfig.requireSecret('mongo_MONGODB_ROOT_PASSWORD'),
});
