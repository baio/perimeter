import {
    createApiDeployment,
    createApiImage,
    createApiLoadBalancer,
} from './api';
import { AppConfig } from './app-config';

export const createK8sCluster = (version: string, config: AppConfig) => {
    // api
    const apiImageFolder = '../../IdentityServer';
    const apiImageName = 'baio/prr-api';

    const apiImage = createApiImage(
        apiImageFolder,
        apiImageName,
        version,
        config.registry,
    );

    const apiAppName = 'prr-api';

    const apiDeployment = createApiDeployment(
        apiAppName,
        config.api.env,
        apiImage.imageName,
    );
    const apiLoadBalancer = createApiLoadBalancer(apiAppName, config.api.ports);

    return {
        apiImageName: apiImage.imageName,
        apiDeploymentName: apiDeployment.metadata.name,
        apiLoadBalancerUrn: apiLoadBalancer.urn,
    };
};
