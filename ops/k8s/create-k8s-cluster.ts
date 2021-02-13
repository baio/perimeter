import { ApiConfig, ApiEnv, createAuthApi, createTenantApi } from './api';
import { AppConfig } from './app-config';
import { createMongo } from './db/mongo';
import { create as createJaeger } from './observability/jaeger/create';
import { create as createSeq } from './observability/seq/create';
import { createPsql } from './db/psql';
import { create as createRabbit } from './bus/rabbit/create';
import { create as createNginx } from './nginx-test/nginx-ingress';
import * as k8s from '@pulumi/kubernetes';
import * as pulumi from '@pulumi/pulumi';
import { createStack, createVolumeStack } from './shared';
import {
    getApiAuthEnv,
    getApiTenantEnv,
    getMongoEnv,
    getPasqlEnv,
} from './config';

export const createK8sCluster = () => {
    //const nginx = createNginx();

    const config = new pulumi.Config();

    //return nginx;
    //api
    const apiAuthConfig = getApiAuthEnv(config);
    const apiAuthStack = createStack(
        'api-auth',
        '0.50.3',
        'baio/prr-api-auth',
        apiAuthConfig,
        { port: 80, targetPort: 5000 },
    );
    const apiTenantStack = createStack(
        'api-tenant',
        '0.50.3',
        'baio/prr-api-tenant',
        getApiTenantEnv(config),
        { port: 80, targetPort: 6000 },
    );
    //app
    const appAdminStack = createStack(
        'app-admin',
        '0.30.3',
        'baio/prr-app-admin',
    );
    const appIdpStack = createStack('app-idp', '0.30.3', 'baio/prr-app-idp');

    //db
    const psqlConfig = getPasqlEnv(config);
    const psqlVolumeConfig = {
        storageCapacity: psqlConfig.storageSize,
        hostPaths: psqlConfig.dataPath,
    };
    const psqlStack = createVolumeStack(
        'psql',
        'postgres:11.4',
        psqlVolumeConfig,
        psqlConfig,
        5432,
    );

    const mongoConfig = getMongoEnv(config);
    const mongoVolumeConfig = {
        storageCapacity: mongoConfig.storageSize,
        hostPaths: mongoConfig.dataPath,
    };
    const mongoStack = createVolumeStack(
        'mongo',
        'mongo:4.4.3',
        mongoVolumeConfig,
        mongoConfig,
        27017,
    );

    /*
    const psql = createPsql(config.psql);
    const mongo = createMongo(config.mongo);
    */

    const rabbitStack = createStack(
        'rabbit',
        null,
        'arm64v8/rabbitmq',
        null,
        5672,
    );

    const ingress = new k8s.networking.v1beta1.Ingress('prr-ingress', {
        spec: {
            rules: [
                {
                    host: 'perimeter.pw',
                    http: {
                        paths: [
                            {
                                path: '/oauth',
                                backend: {
                                    serviceName: appIdpStack.nodePortName,
                                    servicePort: 80,
                                },
                            },
                        ],
                    },
                },
                {
                    host: 'perimeter.pw',
                    http: {
                        paths: [
                            {
                                backend: {
                                    serviceName: appAdminStack.nodePortName,
                                    servicePort: 80,
                                },
                            },
                        ],
                    },
                },
            ],
        },
    });

    // const jaeger = createJaeger();
    // const seq = createSeq();
    // prometheus is not setup since it requires add whole persistent volume / claim story to config (insane shit)
    return {
        ingress: ingress.urn,
        apiAuthStack,
        apiTenantStack,
        appIdpStack,
        appAdminStack,
        psqlStack,
        mongoStack,
        rabbitStack,
        // jaeger,
        // seq,
    };
};
