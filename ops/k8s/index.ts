import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';
import * as github from '@pulumi/github';

// https://www.pulumi.com/docs/reference/pkg/docker/image/

// The folder containing a Dockerfile.
const customImage = '../../IdentityServer';
const version = 'v0.21';
const imageName = 'baio/prr-api';

const config = new pulumi.Config();
const registryServer = config.require('registryServer');
const registryUsername = config.require('registryUsername');
const registryPassword = config.require('registryPassword');

const registry: docker.ImageRegistry = {
    server: registryServer,
    username: registryUsername,
    password: registryPassword,
};

const myImage = new docker.Image(customImage, {
    imageName: `${imageName}:${version}`,
    build: {
        context: `./${customImage}`,
    },
    registry,
});
export const name = myImage.imageName;
