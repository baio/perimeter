import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';

const pulumiConfig = new pulumi.Config();
const registryServer = pulumiConfig.require('registryServer');
const registryUsername = pulumiConfig.require('registryUsername');
const registryPassword = pulumiConfig.require('registryPassword');

const registry: docker.ImageRegistry = {
    server: registryServer,
    username: registryUsername,
    password: registryPassword,
};

export const config = {
    registry,
};
