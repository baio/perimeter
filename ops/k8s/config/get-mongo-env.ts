import * as pulumi from '@pulumi/pulumi';

export const getMongoEnv = (pulumiConfig: pulumi.Config) => ({
    MONGO_USER: pulumiConfig.require('mongo_MONGODB_ROOT_USER'),
    MONGO_PASSWORD: pulumiConfig.requireSecret('mongo_MONGODB_ROOT_PASSWORD'),
    storageSize: pulumiConfig.requireNumber('mongo_storageSize'),
    dataPath: pulumiConfig.require('mongo_dataPath'),
});
