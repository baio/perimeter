import * as k8s from '@pulumi/kubernetes';

export const createNodePorts = (appName: string) => {
    const name = `${appName}-node-ports`;
    const volume = new k8s.core.v1.Service(name, {
        metadata: {
            name,
            labels: {
                name,
                app: appName,
            },
        },
        spec: {
            type: 'NodePort',
            ports: [
                {
                    port: 5672,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
