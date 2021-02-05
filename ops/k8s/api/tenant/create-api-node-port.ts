import * as k8s from '@pulumi/kubernetes';
import { ApiConfigPorts } from '../models';

export const createApiNodePort = (name: string, appName: string, config: ApiConfigPorts) => {
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
                    targetPort: config.targetPort,
                },
            ],
            selector: {
                app: appName,
            },
        },
    });
    return volume;
};
