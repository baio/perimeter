import * as pulumi from '@pulumi/pulumi';

export const getApiAuthEnv = (pulumiConfig: pulumi.Config) => {
    const get = (x: string) => pulumiConfig.get<string>('api_ENV_' + x);
    const rq = (x: string) => pulumiConfig.require('api_ENV_' + x);
    const rs = (x: string) => pulumiConfig.requireSecret('api_ENV_' + x);

    return {
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
    };
};
