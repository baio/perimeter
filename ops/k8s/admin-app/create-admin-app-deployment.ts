import * as k8s from '@pulumi/kubernetes';
import { Output } from '@pulumi/pulumi';

export const createAdminAppDeployment = (
    appName: string,
    imageName: Output<string>,
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
                        },
                    ],
                },
            },
        },
    });
    return deployment;
};
