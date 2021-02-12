import { ImageRegistry } from '@pulumi/docker';
import { ApiConfig } from '../models';
import { createApiImage } from './create-api-image';
import { createApiDeployment } from './create-api-deployment';
import { createApiLoadBalancer } from './create-api-load-balancer';
import { createApiNodePort } from './create-api-node-port';
export const createApi = (
    version: string,
    config: ApiConfig,
    imageRegistry: ImageRegistry,
) => {
    /*
    const dockerfileName =
        './../../IdentityServer/Apps/PRR/PRR.API.Auth/dockerfile';
    const apiImageFolder = './../../IdentityServer';
    const apiImageName = 'baio/prr-api-auth';


    const apiImage = createApiImage(
        apiImageFolder,
        dockerfileName,
        apiImageName,
        version,
        imageRegistry,
    );
    */
   const imageName = `baio/prr-api-auth:${version}`;

    const apiAppExtName = 'prr-api-auth-ext';
    const apiAppName = 'prr-api-auth';

    const apiDeployment = createApiDeployment(
        apiAppName,
        config.env,
        imageName,
    );
    /*
    const apiLoadBalancer = createApiLoadBalancer(
        apiAppExtName,
        apiAppName,
        config.ports,
    );
    */
    const nodePort = createApiNodePort(apiAppName, apiAppName, config.ports);
    return {
        apiImageName: imageName,
        apiDeploymentName: apiDeployment.metadata.name,
        // apiLoadBalancerUrn: apiLoadBalancer.urn,
        apiNodePortUrn: nodePort.urn,
    };
};
