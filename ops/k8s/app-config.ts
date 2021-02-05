import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';
import { Config as AdminAppConfig } from './admin-app';
import { ApiConfig } from './api';
import { MongoConfig } from './mongo';
import { PsqlConfig } from './psql';

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

const rq = (x: string) => pulumiConfig.require('api_ENV_' + x);
const rs = (x: string) => pulumiConfig.requireSecret('api_ENV_' + x);

const authApiConfig: ApiConfig = {
    ports: {
        port: pulumiConfig.requireNumber('auth_api_Port'),
        targetPort: pulumiConfig.requireNumber('auth_api_TargetPort'),
    },
    env: {
        ASPNETCORE_ENVIRONMENT: rq('ASPNETCORE_ENVIRONMENT'),
        ConnectionStrings__PostgreSQL: rs('ConnectionStrings__PostgreSQL'),
        MongoKeyValueStorage__ConnectionString: rs(
            'MongoKeyValueStorage__ConnectionString',
        ),
        MongoViewStorage__ConnectionString: rs(
            'MongoViewStorage__ConnectionString',
        ),
        Auth__PasswordSecret: rs('Auth__PasswordSecret'),
        Auth__Jwt__IdTokenSecret: rs('Auth__Jwt__IdTokenSecret'),
        Auth__Jwt__AccessTokenSecret: rs('Auth__Jwt__AccessTokenSecret'),
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
        SendGridApiKey: rs('SendGridApiKey'),
        TenantAuth__AccessTokenSecret: rs('TenantAuth__AccessTokenSecret'),
    },
};

const tenantApiConfig: ApiConfig = {
    ports: {
        port: pulumiConfig.requireNumber('tenant_api_Port'),
        targetPort: pulumiConfig.requireNumber('tenant_api_TargetPort'),
    },
    env: authApiConfig.env
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
}
//
export const appConfig: AppConfig = {
    registry,
    authApi: authApiConfig,
    tenantApi: tenantApiConfig,
    psql: psqlConfig,
    mongo: mongoConfig,
    adminApp: adminAppConfig,
};
