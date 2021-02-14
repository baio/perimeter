import * as k8s from '@pulumi/kubernetes';

export interface NodePortConfig {
    port: number;
    targetPort?: number;
    protocol?: 'TCP' | 'UDP' | 'SCTP';
    name?: string;
}

const mapPort = (port: number | NodePortConfig): NodePortConfig =>
    typeof port === 'number'
        ? {
              port,
              name: port.toString(),
          }
        : { ...port, name: port.name || port.port.toString() };

export type NodePort = number | number[] | NodePortConfig | NodePortConfig[];

export const createNodePort = (
    name: string,
    appName: string,
    port: NodePort = 80,
) => {
    let ports: NodePortConfig[] = [];
    if (Array.isArray(port)) {
        ports = (port as any[]).map((p) => mapPort(p));
    } else if (port) {
        ports = [mapPort(port)];
    }
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
            ports,
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
