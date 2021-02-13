import * as k8s from '@pulumi/kubernetes';
import { PsqlConfig } from './models/psql-env';

export const createApiPsqlDeployment = (
    appName: string,
    claimName: string,
    config: PsqlConfig,
) => {
    const prrApiLabels = { app: appName };
    const prrApiDeployment = new k8s.apps.v1.Deployment(appName, {
        spec: {
            selector: { matchLabels: prrApiLabels },
            replicas: 1,
            template: {
                metadata: { labels: prrApiLabels },
                spec: {
                    containers: [
                        {
                            name: 'postgres',
                            image: 'postgres:11.4',
                            imagePullPolicy: 'IfNotPresent',
                            volumeMounts: [
                                {
                                    mountPath: '/var/lib/postgresql/data',
                                    name: 'postgresdb',
                                },
                            ],
                            env: [
                                {
                                    name: 'POSTGRES_DB',
                                    value: config.POSTGRES_DB,
                                },
                                {
                                    name: 'POSTGRES_USER',
                                    value: config.POSTGRES_USER,
                                },
                                {
                                    name: 'POSTGRES_PASSWORD',
                                    value: config.POSTGRES_PASSWORD,
                                },
                            ],
                        },
                    ],
                    volumes: [
                        {
                            name: 'postgresdb',
                            persistentVolumeClaim: {
                                claimName,
                            },
                        },
                    ],
                },
            },
        },
    });
    return prrApiDeployment;
};
