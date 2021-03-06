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
        TenantAuth__AccessTokenSecret: rs('TenantAuth__AccessTokenSecret'),
        Logging__Seq__ServiceUrl: get('Logging__Seq__ServiceUrl'),
        Tracing__Jaeger__AgentHost: get('Tracing__Jaeger__AgentHost'),
        Logging__Elastic__ServiceUrl: get('Logging__Elastic__ServiceUrl'),
        ServiceBus__Host: get('ServiceBus__Host'),
        MailSender__FromEmail: rq('MailSender__FromEmail'),
        MailSender__FromName: rq('MailSender__FromName'),
        MailSender__Project__BaseUrl: rq('MailSender__Project__BaseUrl'),
        MailSender__Project__Name: rq('MailSender__Project__Name'),
        MailGun__DomainName: rq('MailGun__DomainName'),
        MailGun__ApiKey: rq('MailGun__ApiKey'),
        MailGun__Region: rq('MailGun__Region'),
        Auth__Social__CallbackUrl: rq('Auth__Social__CallbackUrl'),
        Auth__Social__CallbackExpiresInMilliseconds: rq(
            'Auth__Social__CallbackExpiresInMilliseconds',
        ),
        IssuerBaseUrl: rq('IssuerBaseUrl'),
        Auth__LoginPageDomain: rq('Auth__LoginPageDomain'),
    };
};
