import { ImageRegistry } from '@pulumi/docker';
import { createIdpAppDeployment } from './create-idp-app-deployment';
import { createIdpAppImage } from './create-idp-app-image';
import { createIdpAppLoadBalancer } from './create-idp-app-load-balancer';
import { Config } from './models';

export const createIdpApp = (
    version: string,
    config: Config,
    imageRegistry: ImageRegistry,
) => {
    const imageDockerPath = '../../WebClients/apps/idp/dockerfile';
    const imageFolder = '../../WebClients';
    const imageName = 'baio/prr-idp-app';

    const image = createIdpAppImage(
        imageDockerPath,
        imageFolder,
        imageName,
        version,
        imageRegistry,
    );

    const extName = 'prr-idp-app-ext';
    const appName = 'prr-idp-app';

    const deployment = createIdpAppDeployment(appName, image.imageName);

    const loadBalancer = createIdpAppLoadBalancer(
        extName,
        appName,
        config.ports,
    );

    return {
        imageName: image.imageName,
        deploymentName: deployment.metadata.name,
        loadBalancerUrn: loadBalancer.urn,
    };
};
