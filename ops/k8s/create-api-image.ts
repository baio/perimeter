import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';

export const createApiImage = (
    version: string,
    registry: docker.ImageRegistry,
) => {
    const customImage = '../../IdentityServer';
    const imageName = 'baio/prr-api';

    const myImage = new docker.Image(customImage, {
        imageName: `${imageName}:${version}`,
        build: {
            context: `./${customImage}`,
        },
        registry,
    });
    return myImage.imageName;
};
