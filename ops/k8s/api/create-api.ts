import { ImageRegistry } from '@pulumi/docker';
import {
    createApiDeployment,
    createApiImage,
    createApiLoadBalancer,
    createApiNodePort,
    ApiConfig,
} from '.';

export const createApi = (
    version: string,
    config: ApiConfig,
    imageRegistry: ImageRegistry,
) => {
    const apiImageFolder = '../../IdentityServer';
    const apiImageName = 'baio/prr-api';

    const apiImage = createApiImage(
        apiImageFolder,
        apiImageName,
        version,
        imageRegistry,
    );

    const apiAppExtName = 'prr-api-ext';
    const apiAppName = 'prr-api';

    const apiDeployment = createApiDeployment(
        apiAppName,
        config.env,
        apiImage.imageName,
    );
    const apiLoadBalancer = createApiLoadBalancer(
        apiAppExtName,
        apiAppName,
        config.ports,
    );
    const nodePort = createApiNodePort(apiAppName, apiAppName);
    return {
        apiImageName: apiImage.imageName,
        apiDeploymentName: apiDeployment.metadata.name,
        apiLoadBalancerUrn: apiLoadBalancer.urn,
        apiNodePortUrn: nodePort.urn,
    };
};
