import * as k8s from '@pulumi/kubernetes';
import * as pulumi from '@pulumi/pulumi';
import { createDeployment } from './create-deployment';
import { createNodePort, NodePortConfig } from './create-node-port';

export const createStack = (
    appName: string,
    version: string | null,
    imageName: string,
    env?: any, //pulumi.Input<k8s.types.input.core.v1.EnvVar>[],
    port?: number | NodePortConfig,
) => {
    imageName = version ? imageName + ':' + version : imageName;
    const deployment = createDeployment(appName, imageName, env);
    const nodePort = createNodePort(appName, appName, port || 80);
    return {
        imageName: imageName,
        deploymentName: deployment.metadata.name,
        nodePortName: appName,
        nodePortUrn: nodePort.urn,
    };
};
