import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';
import { Config as AdminAppConfig } from './app/admin-app';
import { Config as IdpAppConfig } from './app/idp-app';
import { ApiConfig } from './api';
import { MongoConfig } from './db/mongo';
import { PsqlConfig } from './db/psql';

const pulumiConfig = new pulumi.Config();

//
const registryServer = pulumiConfig.require('registryServer');
const registryUsername = pulumiConfig.require('registryUsername');
const registryPassword = pulumiConfig.requireSecret('registryPassword');

const registry: docker.ImageRegistry = {
    server: registryServer,
    username: registryUsername,
    password: registryPassword,
};

//

const get = (x: string) => pulumiConfig.get<string>('api_ENV_' + x);
const rq = (x: string) => pulumiConfig.require('api_ENV_' + x);
const rs = (x: string) => pulumiConfig.requireSecret('api_ENV_' + x);

const authApiConfig: ApiConfig = {
    ports: {
        port: pulumiConfig.requireNumber('auth_api_Port'),
        targetPort: pulumiConfig.requireNumber('auth_api_TargetPort'),
    },
    env: {
        ASPNETCORE_ENVIRONMENT: rq('ASPNETCORE_ENVIRONMENT'),
        ConnectionStrings__PostgreSQL: rq('ConnectionStrings__PostgreSQL'),
        MongoKeyValueStorage__ConnectionString: rq(
            'MongoKeyValueStorage__ConnectionString',
        ),
        MongoViewStorage__ConnectionString: rq(
            'MongoViewStorage__ConnectionString',
        ),
        Auth__PasswordSecret: rq('Auth__PasswordSecret'),
        Auth__Jwt__IdTokenSecret: rq('Auth__Jwt__IdTokenSecret'),
        Auth__Jwt__AccessTokenSecret: rq('Auth__Jwt__AccessTokenSecret'),
        Auth__PerimeterSocialProviders__Github__ClientId: rs(
            'Auth__PerimeterSocialProviders__Github__ClientId',
        ),
        Auth__PerimeterSocialProviders__Github__SecretKey: rs(
            'Auth__PerimeterSocialProviders__Github__SecretKey',
        ),
        Auth__PerimeterSocialProviders__Google__ClientId: rs(
            'Auth__PerimeterSocialProviders__Google__ClientId',
        ),
        Auth__PerimeterSocialProviders__Google__SecretKey: rs(
            'Auth__PerimeterSocialProviders__Google__SecretKey',
        ),
        SendGridApiKey: rq('SendGridApiKey'),
        TenantAuth__AccessTokenSecret: rs('TenantAuth__AccessTokenSecret'),
        Logging__Seq__ServiceUrl: get('Logging__Seq__ServiceUrl'),
        Tracing__Jaeger__AgentHost: get('Tracing__Jaeger__AgentHost'),
    },
};

const tenantApiConfig: ApiConfig = {
    ports: {
        port: pulumiConfig.requireNumber('tenant_api_Port'),
        targetPort: pulumiConfig.requireNumber('tenant_api_TargetPort'),
    },
    env: authApiConfig.env,
};

const psqlConfig: PsqlConfig = {
    POSTGRES_DB: pulumiConfig.require('psql_POSTGRES_DB'),
    POSTGRES_USER: pulumiConfig.require('psql_POSTGRES_USER'),
    POSTGRES_PASSWORD: pulumiConfig.requireSecret('psql_POSTGRES_PASSWORD'),
    storageSize: pulumiConfig.requireNumber('psql_storageSize'),
    dataPath: pulumiConfig.require('psql_dataPath'),
};

const mongoConfig: MongoConfig = {
    MONGO_USER: pulumiConfig.require('mongo_MONGODB_ROOT_USER'),
    MONGO_PASSWORD: pulumiConfig.requireSecret('mongo_MONGODB_ROOT_PASSWORD'),
    storageSize: pulumiConfig.requireNumber('mongo_storageSize'),
    dataPath: pulumiConfig.require('mongo_dataPath'),
};

const idpAppConfig: AdminAppConfig = {
    ports: {
        port: pulumiConfig.requireNumber('idpApp_Port'),
        targetPort: pulumiConfig.requireNumber('idpApp_TargetPort'),
    },
};

const adminAppConfig: AdminAppConfig = {
    ports: {
        port: pulumiConfig.requireNumber('adminApp_Port'),
        targetPort: pulumiConfig.requireNumber('adminApp_TargetPort'),
    },
};

//
export interface AppConfig {
    registry: docker.ImageRegistry;
    authApi: ApiConfig;
    tenantApi: ApiConfig;
    psql: PsqlConfig;
    mongo: MongoConfig;
    adminApp: AdminAppConfig;
    idpApp: IdpAppConfig;
}
//
export const appConfig: AppConfig = {
    registry,
    authApi: authApiConfig,
    tenantApi: tenantApiConfig,
    psql: psqlConfig,
    mongo: mongoConfig,
    adminApp: adminAppConfig,
    idpApp: idpAppConfig,
};
