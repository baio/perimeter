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
                            name: 'seq',
                            image: 'datalust/seq:latest',
                            imagePullPolicy: 'IfNotPresent',
                            env: [
                                {
                                    name: 'ACCEPT_EULA',
                                    value: 'Y',
                                },
                            ],
                        },
                    ],
                },
            },
        },
    });
    return prrApiDeployment;
};
