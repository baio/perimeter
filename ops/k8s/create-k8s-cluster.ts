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
    getMongoConfig,
    getMongoEnv,
    getPasqlEnv,
    getPsqlConfig,
} from './config';

export const createK8sCluster = () => {
    //const nginx = createNginx();

    const config = new pulumi.Config();

    //return nginx;
    //api

    const apiAuthConfig = getApiAuthEnv(config);
    const apiAuthStack = createStack(
        'prr-api-auth',
        '0.50.3',
        'baio/prr-api-auth',
        apiAuthConfig,
        { port: 80, targetPort: 5000 },
    );

    const apiTenantStack = createStack(
        'prr-api-tenant',
        '0.50.4',
        'baio/prr-api-tenant',
        getApiTenantEnv(config),
        { port: 80, targetPort: 6000 },
    );

    //app
    const appAdminStack = createStack(
        'prr-app-admin',
        '0.30.20',
        'baio/prr-app-admin',
    );
    const appIdpStack = createStack(
        'prr-app-idp',
        '0.30.20',
        'baio/prr-app-idp',
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
                                backend: {
                                    serviceName: appAdminStack.nodePortName,
                                    servicePort: 80,
                                },
                            },
                        ],
                    },
                },
                {
                    host: 'oauth.perimeter.pw',
                    http: {
                        paths: [
                            {
                                backend: {
                                    serviceName: appIdpStack.nodePortName,
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
        ingressUrl: ingress.urn,
        psqlStack,
        apiAuthStack,
        apiTenantStack,
        mongoStack,
        appIdpStack,
        appAdminStack,
        rabbitStack,
        /*
        ingress: ingress.urn,        
        apiTenantStack,
        psqlStack,        
        rabbitStack,
        */
        // jaeger,
        // seq,
    };
};
