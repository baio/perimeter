import * as k8s from '@pulumi/kubernetes';

export const createApiNodePort = (name: string, appName: string) => {
    const volume = new k8s.core.v1.Service(name, {
        metadata: {
            name: name,
            labels: {
                name: name,
                app: appName,
            },
        },
        spec: {
            type: 'NodePort',
            ports: [
                {
                    port: 80,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
