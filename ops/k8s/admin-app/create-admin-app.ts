import { ImageRegistry } from '@pulumi/docker';
import { createAdminAppDeployment } from './create-admin-app-deployment';
import { createAdminAppImage } from './create-admin-app-image';
import { createAdminAppLoadBalancer } from './create-admin-app-load-balancer';
import { Config } from './models';

export const createAdminApp = (
    version: string,
    config: Config,
    imageRegistry: ImageRegistry,
) => {
    /*
    const imageDockerPath = '../../WebClients/apps/admin/dockerfile';
    const imageFolder = '../../WebClients';
    const imageName = 'baio/prr-admin-app';

    const image = createAdminAppImage(
        imageDockerPath,
        imageFolder,
        imageName,
        version,
        imageRegistry,
    );
    */
   const imageName = `baio/prr-app-admin:${version}`;

    const extName = 'prr-app-admin-ext';
    const appName = 'prr-app-admin';

    const deployment = createAdminAppDeployment(appName, imageName);

    const loadBalancer = createAdminAppLoadBalancer(
        extName,
        appName,
        config.ports,
    );

    return {
        imageName: imageName,
        deploymentName: deployment.metadata.name,
        loadBalancerUrn: loadBalancer.urn,
    };
};
