import * as docker from '@pulumi/docker';

export const createApiImage = (
    customImageFolder: string,
    imageName: string,
    version: string,
    registry: docker.ImageRegistry,
) => {
    const myImage = new docker.Image(imageName, {
        imageName: `${imageName}:${version}`,
        build: {
            context: `./${customImageFolder}`,
        },
        registry,
    });
    return myImage;
};
