import * as docker from '@pulumi/docker';

export const createIdpAppImage = (
    dockerFilePath: string,
    customImageFolder: string,
    imageName: string,
    version: string,
    registry: docker.ImageRegistry,
) => {
    const myImage = new docker.Image(imageName, {
        imageName: `${imageName}:${version}`,
        build: {
            dockerfile: dockerFilePath,
            context: `./${customImageFolder}`,
        },
        registry,
    });
    return myImage;
};
