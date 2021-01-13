import {
    createApiDeployment,
    createApiImage,
    createApiLoadBalancer,
} from './api';
import { AppConfig } from './app-config';
import {
    createApiMongoClaim,
    createApiMongoDeployment,
    createApiMongoVolume,
    createApiMongoNodePort,
    MongoConfig,
} from './mongo';
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

const createMongo = (config: MongoConfig) => {
    const psqlAppName = 'mongo';
    const volumeName = 'mongo-pv-volume';
    const claimName = 'mongo-pv-claim';
    const volume = createApiMongoVolume(psqlAppName, volumeName, {
        storageCapacity: config.storageSize,
        hostPaths: config.dataPath,
    });
    const claim = createApiMongoClaim(psqlAppName, claimName, {
        requestsStorage: config.storageSize,
    });
    const deployment = createApiMongoDeployment(psqlAppName, claimName, config);
    const nodePort = createApiMongoNodePort(psqlAppName);
    return {
        volume: volume.urn,
        claim: claim.urn,
        deployment: deployment.urn,
        nodePort: nodePort.urn,
    };
};

export const createK8sCluster = (version: string, config: AppConfig) => {
    const api = createApi(version, config);
    const psql = createPsql(config.psql);
    const mongo = createMongo(config.mongo);
    return {
        api,
        psql,
        mongo,
    };
};
