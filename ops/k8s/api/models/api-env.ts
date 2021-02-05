import { Output } from '@pulumi/pulumi';

export interface ApiEnv {
    ASPNETCORE_ENVIRONMENT: string;
    ConnectionStrings__PostgreSQL: Output<string>;
    MongoKeyValueStorage__ConnectionString: Output<string>;
    MongoViewStorage__ConnectionString: Output<string>;
    Auth__PasswordSecret: Output<string>;
    Auth__Jwt__IdTokenSecret: Output<string>;
    Auth__Jwt__AccessTokenSecret: Output<string>;
    Auth__PerimeterSocialProviders__Github__ClientId: Output<string>;
    Auth__PerimeterSocialProviders__Github__SecretKey: Output<string>;
    Auth__PerimeterSocialProviders__Google__ClientId: Output<string>;
    Auth__PerimeterSocialProviders__Google__SecretKey: Output<string>;
    SendGridApiKey: Output<string>;
    TenantAuth__AccessTokenSecret: Output<string>;
}
