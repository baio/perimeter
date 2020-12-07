import * as docker from '@pulumi/docker';
import * as pulumi from '@pulumi/pulumi';
import { ApiConfig } from './api';

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

const apiConfig: ApiConfig = {
    ports: {
        port: pulumiConfig.requireNumber('apiPort'),
        targetPort: pulumiConfig.requireNumber('apiTargetPort'),
    },
    env: {
        ASPNETCORE_ENVIRONMENT: rq('ASPNETCORE_ENVIRONMENT'),
        ConnectionStrings__PostgreSQL: rs('ConnectionStrings__PostgreSQL'),
        ConnectionStrings__MongoJournal: rs('ConnectionStrings__MongoJournal'),
        ConnectionStrings__MongoViews: rs('ConnectionStrings__MongoViews'),
        Auth__PasswordSecret: rs('Auth__PasswordSecret'),
        Auth__Jwt__IdTokenSecret: rs('Auth__Jwt__IdTokenSecret'),
        Auth__Jwt__AccessTokenSecret: rs('Auth__Jwt__IdTokenSecret'),
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
    },
};

//
export interface AppConfig {
    registry: docker.ImageRegistry;
    api: ApiConfig;
}
//
export const appConfig: AppConfig = {
    registry,
    api: apiConfig,
};
