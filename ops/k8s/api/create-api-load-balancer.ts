import * as k8s from '@pulumi/kubernetes';
import { ApiConfigPorts } from './models';

export const createApiLoadBalancer = (
    appName: string,
    config: ApiConfigPorts,
) => {
    const prrApiLabels = { app: appName };
    const prrIpLoadBalancer = new k8s.core.v1.Service(appName, {
        metadata: {
            name: appName,
            labels: {
                name: appName,
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
