import * as k8s from '@pulumi/kubernetes';

export interface NodePortConfig {
    port: number;
    targetPort?: number;
}

export const createNodePort = (
    name: string,
    appName: string,
    port: number | NodePortConfig = 80,
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
                typeof port === 'number'
                    ? {
                          port,
                      }
                    : port,
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
