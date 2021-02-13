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

    /*
    const imageDockerPath = '../../WebClients/apps/idp/dockerfile';
    const imageFolder = '../../WebClients';
    const imageName = 'baio/prr-app-idp';

    const image = createIdpAppImage(
        imageDockerPath,
        imageFolder,
        imageName,
        version,
        imageRegistry,
    );
    */
    
    const imageName = `baio/prr-app-idp:${version}`;

    version = '0.30.20';    
    const appName = 'prr-app-idp';

    const deployment = createIdpAppDeployment(appName, imageName);

    /*
    const extName = 'prr-app-idp-ext';
    const loadBalancer = createIdpAppLoadBalancer(
        extName,
        appName,
        config.ports,
    );
    */

    return {
        imageName: imageName,
        deploymentName: deployment.metadata.name,
        // loadBalancerUrn: loadBalancer.urn,
    };
};
