import * as docker from '@pulumi/docker';

export const createApiImage = (
    customImageFolder: string,
    customDockerfileName: string,
    imageName: string,
    version: string,
    registry: docker.ImageRegistry,
) => {
    const myImage = new docker.Image(imageName, {
        imageName: `${imageName}:${version}`,
        build: {
            dockerfile: customDockerfileName,
            context: customImageFolder,
        },
        registry,
    });
    return myImage;
};
