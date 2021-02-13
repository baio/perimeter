import { Output } from '@pulumi/pulumi';

export interface ApiEnv {
    ASPNETCORE_ENVIRONMENT: string;
    ConnectionStrings__PostgreSQL: string | Output<string>;
    MongoKeyValueStorage__ConnectionString: string | Output<string>;
    MongoViewStorage__ConnectionString: string | Output<string>;
    Auth__PasswordSecret: string | Output<string>;
    Auth__Jwt__IdTokenSecret: string | Output<string>;
    Auth__Jwt__AccessTokenSecret: string | Output<string>;
    Auth__PerimeterSocialProviders__Github__ClientId: string | Output<string>;
    Auth__PerimeterSocialProviders__Github__SecretKey: string | Output<string>;
    Auth__PerimeterSocialProviders__Google__ClientId: string | Output<string>;
    Auth__PerimeterSocialProviders__Google__SecretKey: string | Output<string>;
    SendGridApiKey: string | Output<string>;
    TenantAuth__AccessTokenSecret: string | Output<string>;
    Logging__Seq__ServiceUrl: string | undefined;
    Tracing__Jaeger__AgentHost: string | undefined;
}
