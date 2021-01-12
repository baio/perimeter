import {
    createApiDeployment,
    createApiImage,
    createApiLoadBalancer,
} from './api';
import { AppConfig } from './app-config';
import {
    createApiPsqlClaim,
    createApiPsqlDeployment,
    createApiPsqlNodePort,
    createApiPsqlVolume,
    PsqlConfig,
} from './psql';

const createApi = (version: string, config: AppConfig) => {
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

const createPsql = (config: PsqlConfig) => {
    const psqlAppName = 'psql';
    const volumeName = 'postgres-pv-volume';
    const claimName = 'postgres-pv-claim';
    const volume = createApiPsqlVolume(psqlAppName, volumeName, {
        storageCapacity: config.storageSize,
        hostPaths: config.dataPath,
    });
    const claim = createApiPsqlClaim(psqlAppName, claimName, {
        requestsStorage: config.storageSize,
    });
    const deployment = createApiPsqlDeployment(psqlAppName, claimName, config);
    const nodePort = createApiPsqlNodePort(psqlAppName);
    return {
        volume: volume.urn,
        claim: claim.urn,
        deployment: deployment.urn,
        nodePort: nodePort.urn,
    };
};

export const createK8sCluster = (version: string, config: AppConfig) => {
    //const api = createApi(version, config);
    const psql = createPsql(config.psql);
    return psql;
};
