import * as docker from '@pulumi/docker';

export const createImage = (
    dockerfile: string,
    context: string,
    imageName: string,
    version: string,
    registry: docker.ImageRegistry,
) => {
    const myImage = new docker.Image(imageName, {
        imageName: `${imageName}:${version}`,
        build: {
            dockerfile: dockerfile,
            context: `./${context}`,
        },
        registry,
    });
    return myImage;
};
