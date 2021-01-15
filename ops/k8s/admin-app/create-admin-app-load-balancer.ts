import * as k8s from '@pulumi/kubernetes';
import { ConfigPorts } from './models';

export const createAdminAppLoadBalancer = (
    name: string,
    appName: string,
    config: ConfigPorts,
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
                    port: config.port,
                    targetPort: config.targetPort,
                },
            ],
            selector: prrApiLabels,
        },
    });

    return prrIpLoadBalancer;
};
