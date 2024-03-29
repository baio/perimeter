import * as k8s from '@pulumi/kubernetes';

export const createLoadBalancer = (appName: string) => {
    const name = `${appName}-load-balancer`;
    const prrApiLabels = { app: appName };
    const prrIpLoadBalancer = new k8s.core.v1.Service(name, {
        metadata: {
            name,
            labels: {
                name,
                app: appName,
            },
        },
        spec: {
            type: 'LoadBalancer',
            ports: [
                {
                    port: 16687,
                    targetPort: 16686,
                },
            ],
            selector: prrApiLabels,
        },
    });

    return prrIpLoadBalancer;
};
