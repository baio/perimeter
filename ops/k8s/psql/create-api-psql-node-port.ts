import * as k8s from '@pulumi/kubernetes';

export const createApiPsqlNodePort = (appName: string) => {
    const volume = new k8s.core.v1.Service(appName, {
        metadata: {
            labels: {
                name: appName,
                app: appName,
            },
        },
        spec: {
            type: 'NodePort',
            ports: [
                {
                    port: 5432,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
