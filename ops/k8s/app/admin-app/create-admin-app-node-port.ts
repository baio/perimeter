import * as k8s from '@pulumi/kubernetes';

export const createAdminAppNodePort = (
    name: string,
    appName: string,
    targetPort = 80,
) => {
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
                    targetPort,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
