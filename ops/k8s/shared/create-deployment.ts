import * as k8s from '@pulumi/kubernetes';
import { Output } from '@pulumi/pulumi';
import * as pulumi from '@pulumi/pulumi';

export const createDeployment = (
    appName: string,
    imageName: Output<string> | string,
    env?: any, //pulumi.Input<k8s.types.input.core.v1.EnvVar>[],
) => {
    const labels = { app: appName };
    const deployment = new k8s.apps.v1.Deployment(appName, {
        spec: {
            selector: { matchLabels: labels },
            replicas: 1,
            template: {
                metadata: { labels: labels },
                spec: {
                    containers: [
                        {
                            name: appName,
                            image: imageName,
                            env,
                        },
                    ],
                },
            },
        },
    });
    return deployment;
};
