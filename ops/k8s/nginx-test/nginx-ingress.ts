import * as k8s from '@pulumi/kubernetes';

export const create = () => {
    const name = 'nginx';

    const labels = { app: name };
    const deployment = new k8s.apps.v1.Deployment(name, {
        spec: {
            selector: { matchLabels: labels },
            replicas: 1,
            template: {
                metadata: { labels: labels },
                spec: {
                    containers: [
                        {
                            name: name,
                            image: name,
                        },
                    ],
                },
            },
        },
    });

    const service = new k8s.core.v1.Service(name, {
        metadata: {
            name: name,
            labels: {
                name: name,
                app: name,
            },
        },
        spec: {
            type: 'NodePort',
            ports: [
                {
                    port: 80,
                    targetPort: 80,
                },
            ],
            selector: {
                app: name,
            },
        },
    });

    const ingress = new k8s.networking.v1beta1.Ingress(name, {
        spec: {
            rules: [
                {
                    host: 'perimeter.pw',
                    http: {
                        paths: [
                            {
                                backend: {
                                    serviceName: name,
                                    servicePort: 80,
                                },
                            },
                        ],
                    },
                },
            ],
        },
    });

    return {
        deployment,
        service,
        ingress,
    };
};
