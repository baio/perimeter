import * as k8s from '@pulumi/kubernetes';

export const createDeployment = (appName: string) => {
    const prrApiLabels = { app: appName };
    const prrApiDeployment = new k8s.apps.v1.Deployment(appName, {
        spec: {
            selector: { matchLabels: prrApiLabels },
            replicas: 1,
            template: {
                metadata: { labels: prrApiLabels },
                spec: {
                    containers: [
                        {
                            name: 'rabbitmq',
                            image: 'masstransit/rabbitmq',
                            imagePullPolicy: 'IfNotPresent',
                        },
                    ],
                },
            },
        },
    });
    return prrApiDeployment;
};
