import * as k8s from '@pulumi/kubernetes';

export interface VolumeConfig {
    storageCapacity: number;
    hostPaths: string;
}

export const createVolume = (
    appName: string,
    volumeName: string,
    config: VolumeConfig,
) => {
    const volume = new k8s.core.v1.PersistentVolume(volumeName, {
        metadata: {
            name: volumeName,
            labels: {
                type: 'local',
                app: appName,
            },
        },
        spec: {
            storageClassName: 'manual',
            capacity: { storage: config.storageCapacity + 'Gi' },
            accessModes: ['ReadWriteMany'],
            hostPath: {
                path: config.hostPaths,
            },
        },
    });
    return volume;
};
