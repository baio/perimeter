import * as k8s from '@pulumi/kubernetes';
import { Output } from '@pulumi/pulumi';

export const createIdpAppIngress = (
    appName: string,
    imageName: Output<string> | string,
) => {
    const labels = { app: appName };
    const deployment = new k8s.networking.v1beta1.Ingress(appName, {
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
