import {
    createApiMongoClaim,
    createApiMongoDeployment,
    createApiMongoNodePort,
    createApiMongoVolume,
    MongoConfig,
} from '.';

export const createMongo = (config: MongoConfig) => {
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

