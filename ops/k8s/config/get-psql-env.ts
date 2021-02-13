import * as pulumi from '@pulumi/pulumi';

export const getPsqlConfig = (pulumiConfig: pulumi.Config) => ({
    storageCapacity: pulumiConfig.requireNumber('psql_storageSize'),
    hostPaths: pulumiConfig.require('psql_dataPath'),
});

export const getPasqlEnv = (pulumiConfig: pulumi.Config) => ({
    POSTGRES_DB: pulumiConfig.require('psql_POSTGRES_DB'),
    POSTGRES_USER: pulumiConfig.require('psql_POSTGRES_USER'),
    POSTGRES_PASSWORD: pulumiConfig.requireSecret('psql_POSTGRES_PASSWORD'),
});
