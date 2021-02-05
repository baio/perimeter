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
                            name: 'prometheus',
                            image: 'prom/prometheus',
                            imagePullPolicy: 'IfNotPresent',
                            volumeMounts: [
                                {                                
                                    mountPath: '/etc/prometheus/prometheus.yml',
                                    name: 'prometheus',
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
