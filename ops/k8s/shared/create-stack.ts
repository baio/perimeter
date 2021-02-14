import { createDeployment } from './create-deployment';
import { createNodePort, NodePort } from './create-node-port';

export const createStack = (
    appName: string,
    version: string | null,
    imageName: string,
    envs?: any, //pulumi.Input<k8s.types.input.core.v1.EnvVar>[],
    port?: NodePort,
) => {
    imageName = version ? imageName + ':' + version : imageName;
    const deployment = createDeployment(appName, imageName, envs);
    const nodePort = createNodePort(appName, appName, port || 80);
    return {
        imageName: imageName,
        deploymentName: deployment.metadata.name,
        nodePortName: appName,
        nodePortUrn: nodePort.urn,
    };
};
