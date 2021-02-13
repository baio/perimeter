import * as k8s from '@pulumi/kubernetes';
import { MongoConfig } from './models';

export const createApiMongoDeployment = (
    appName: string,
    claimName: string,
    config: MongoConfig,
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
                            name: 'mongo',
                            image: 'mongo:4.4.3',
                            imagePullPolicy: 'IfNotPresent',
                            volumeMounts: [
                                {
                                    mountPath: '/etc/mongo',
                                    name: 'mongo',
                                },
                            ],
                            env: [
                                {
                                    name: 'MONGO_INITDB_ROOT_USERNAME',
                                    value: config.MONGO_USER,
                                },
                                {
                                    name: 'MONGO_INITDB_ROOT_PASSWORD',
                                    value: config.MONGO_PASSWORD,
                                },
                            ],
                        },
                    ],
                    volumes: [
                        {
                            name: 'mongo',
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
