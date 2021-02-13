import * as k8s from '@pulumi/kubernetes';
import * as pulumi from '@pulumi/pulumi';
import { createDeployment } from './create-deployment';
import { createNodePort } from './create-node-port';
import { createStack } from './create-stack';
import { createVolume, VolumeConfig } from './create-volume';
import { createVolumeClaim } from './create-volume-claim';

export const createVolumeStack = (
    appName: string,
    fullImageName: string,
    config: VolumeConfig,
    env?: any, //pulumi.Input<k8s.types.input.core.v1.EnvVar>[],
    port?: number,
) => {
    const volumeName = `${appName}-pv-volume`;
    const claimName = `${appName}-pv-claim`;
    const volume = createVolume(appName, volumeName, {
        storageCapacity: config.storageCapacity,
        hostPaths: config.hostPaths,
    });
    const claim = createVolumeClaim(appName, claimName, {
        requestsStorage: config.storageCapacity,
    });

    const stack = createStack(appName, null, fullImageName, env, port);
    return {
        ...stack,
        volumeName: volumeName,
        volumeUrn: volume.urn,
        claimName: claimName,
        claimUrn: claim.urn,
    };
};
