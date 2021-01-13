import * as k8s from '@pulumi/kubernetes';

export const createApiMongoNodePort = (appName: string) => {
    const volume = new k8s.core.v1.Service(appName, {
        metadata: {
            name: appName,
            labels: {
                name: appName,
                app: appName,
            },
        },
        spec: {
            type: 'NodePort',
            ports: [
                {
                    port: 27017,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
