import * as pulumi from '@pulumi/pulumi';
import { getApiAuthEnv, getMongoConfig, getMongoEnv, getPasqlEnv, getPsqlConfig } from './config';
import { createStack, createVolumeStack } from './shared';

export const createK8sCluster = () => {
    const config = new pulumi.Config();

    //return nginx;
    //api

    const probe = {
        httpGet: {
            path: '/api/auth/health',
            port: 5000,
        },
        failureThreshold: 5,
        periodSeconds: 10,
    };
    const apiAuthConfig = getApiAuthEnv(config);
    const apiAuthStack = createStack(
        'prr-api-auth',
        'l2',
        'baio/prr-api-auth',
        apiAuthConfig,
        { port: 80, targetPort: 5000 },
        probe,
    );
    //db

    const psqlConfig = getPasqlEnv(config);
    const psqlVolumeConfig = getPsqlConfig(config);
    const psqlStack = createVolumeStack(
        'psql',
        'postgres:11.4',
        psqlVolumeConfig,
        psqlConfig,
        5432,
    );

    const mongoEnv = getMongoEnv(config);
    const mongoVolumeConfig = getMongoConfig(config);
    const mongoStack = createVolumeStack(
        'mongo',
        'mongo:4.4.3',
        mongoVolumeConfig,
        mongoEnv,
        27017,
    );

    // bus

    const rabbitStack = createStack(
        'rabbit',
        null,
        'masstransit/rabbitmq',
        null,
        [5672, 15672],
    );

    return {
        psqlStack,
        apiAuthStack,
        mongoStack,
        rabbitStack,
    };
};
