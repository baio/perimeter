import * as k8s from '@pulumi/kubernetes';

export interface ClaimConfig {
    requestsStorage: number;
}

export const createApiMongoClaim = (
    appName: string,
    claimName: string,
    config: ClaimConfig,
) => {
    const volume = new k8s.core.v1.PersistentVolumeClaim(claimName, {
        metadata: {
            name: claimName,
            labels: {
                app: appName,
            },
        },
        spec: {
            storageClassName: 'manual',
            accessModes: ['ReadWriteMany'],
            resources: {
                requests: {
                    storage: config.requestsStorage + 'Gi',
                },
            },
        },
    });
    return volume;
};
