import * as k8s from '@pulumi/kubernetes';
import * as pulumi from '@pulumi/pulumi';
import {
    getApiAuthEnv,
    getApiTenantEnv,
    getMongoConfig,
    getMongoEnv,
    getPasqlEnv,
    getPsqlConfig,
} from './config';
import { createStack, createVolumeStack } from './shared';

export const createK8sCluster = () => {
    //const nginx = createNginx();

    const config = new pulumi.Config();

    //return nginx;
    //api

    const apiAuthConfig = getApiAuthEnv(config);
    const apiAuthStack = createStack(
        'prr-api-auth',
        '0.50.6',
        'baio/prr-api-auth',
        apiAuthConfig,
        { port: 80, targetPort: 5000 },
    );

    const apiTenantStack = createStack(
        'prr-api-tenant',
        '0.50.7',
        'baio/prr-api-tenant',
        getApiTenantEnv(config),
        { port: 80, targetPort: 6000 },
    );

    //app
    const appAdminStack = createStack(
        'prr-app-admin',
        '0.30.28',
        'baio/prr-app-admin',
    );
    const appIdpStack = createStack(
        'prr-app-idp',
        '0.30.28',
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

    // bus

    const rabbitStack = createStack('rabbit', null, 'arm64v8/rabbitmq', null, [
        5672,
        15672,
    ]);

    // observability
    const elasticStack = createStack(
        'elastic',
        null,
        'docker.elastic.co/elasticsearch/elasticsearch:7.11.0',
        {
            'discovery.type': 'single-node',
        },
        [9200, 9300],
    );

    const jaegerStack = createStack(
        'jaeger',
        null,
        'thomasmatbalenaio/jaegertracing-all-in-one-arm64',
        null,
        [
            { port: 6831, protocol: 'UDP' },
            { port: 6832, protocol: 'UDP' },
            { port: 16686, protocol: 'TCP' },
        ],
    );

    // ingress
    const ingress = new k8s.networking.v1beta1.Ingress('prr-ingress', {
        spec: {
            tls: [
                {
                    hosts: ['*.perimeter.pw'],
                    secretName: 'perimeter-secret-tls',
                },
                {
                    hosts: ['*.prr.pw'],
                    secretName: 'prr-secret-tls',
                },
            ],
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
                    host: 'prr.pw',
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
        elasticStack,
        jaegerStack,
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
