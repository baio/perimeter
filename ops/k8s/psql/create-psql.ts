import {
    createApiPsqlClaim,
    createApiPsqlDeployment,
    createApiPsqlNodePort,
    createApiPsqlVolume,
    PsqlConfig,
} from '.';

export const createPsql = (config: PsqlConfig) => {
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
