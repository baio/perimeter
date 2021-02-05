import * as k8s from '@pulumi/kubernetes';
import { ApiConfigPorts } from '../models';

export const createApiLoadBalancer = (
    name: string,
    appName: string,
    config: ApiConfigPorts,
) => {
    const prrApiLabels = { app: appName };
    const prrIpLoadBalancer = new k8s.core.v1.Service(name, {
        metadata: {
            name: name,
            labels: {
                name: name,
                app: appName,
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
