import { Output } from "@pulumi/pulumi";

export interface ApiEnv {
    ASPNETCORE_ENVIRONMENT: string;
    ConnectionStrings__PostgreSQL: Output<string>;
    ConnectionStrings__MongoJournal: Output<string>;
    ConnectionStrings__MongoViews: Output<string>;
    ConnectionStrings__MongoSnapshot: Output<string>;
    Auth__PasswordSecret: Output<string>;
    Auth__Jwt__IdTokenSecret: Output<string>;
    Auth__Jwt__AccessTokenSecret: Output<string>;
    Auth__PerimeterSocialProviders__Github__ClientId: Output<string>;
    Auth__PerimeterSocialProviders__Github__SecretKey: Output<string>;
    Auth__PerimeterSocialProviders__Google__ClientId: Output<string>;
    Auth__PerimeterSocialProviders__Google__SecretKey: Output<string>;
    SendGridApiKey: Output<string>;
}