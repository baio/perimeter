import * as k8s from '@pulumi/kubernetes';

export type LoadBalancerPorts = {
    port?: number | any;
    targetPort?: number;
};

export const createLoadBalancer = (
    name: string,
    appName: string,
    ports: LoadBalancerPorts,
) => {
    const prrApiLabels = { app: appName };
    const prrIpLoadBalancer = new k8s.core.v1.Service(name, {
        metadata: {
            name: name,
            labels: {
                name: name,
            },
        },
        spec: {
            type: 'LoadBalancer',
            ports: [
                {
                    port: ports.port,
                    targetPort: ports.targetPort,
                },
            ],
            selector: prrApiLabels,
        },
    });

    return prrIpLoadBalancer;
};
