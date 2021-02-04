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
                    name: "6831",
                    port: 6831,
                    protocol: 'UDP',
                },
                {
                    name: "6832",
                    port: 6832,
                    protocol: 'UDP',
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
