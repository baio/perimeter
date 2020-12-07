import * as k8s from '@pulumi/kubernetes';
import { Output } from '@pulumi/pulumi';
import { ApiEnv } from './models';

export const createApiDeployment = (
    appName: string,
    apiEnv: ApiEnv,
    imageName: Output<string>,
) => {
    const prrApiLabels = { app: appName };
    const prrApiDeployment = new k8s.apps.v1.Deployment(appName, {
        spec: {
            selector: { matchLabels: prrApiLabels },
            replicas: 1,
            template: {
                metadata: { labels: prrApiLabels },
                spec: {
                    containers: [
                        {
                            name: appName,
                            image: imageName,
                            env: [
                                {
                                    name: 'ASPNETCORE_ENVIRONMENT',
                                    value: apiEnv.ASPNETCORE_ENVIRONMENT,
                                },
                                {
                                    name: 'ConnectionStrings__PostgreSQL',
                                    value: apiEnv.ConnectionStrings__PostgreSQL,
                                },
                                {
                                    name: 'ConnectionStrings__MongoJournal',
                                    value:
                                        apiEnv.ConnectionStrings__MongoJournal,
                                },
                                {
                                    name: 'ConnectionStrings__MongoViews',
                                    value: apiEnv.ConnectionStrings__MongoViews,
                                },
                                {
                                    name: 'Auth__PasswordSecret',
                                    value: apiEnv.Auth__PasswordSecret,
                                },
                                {
                                    name: 'Auth__Jwt__IdTokenSecret',
                                    value: apiEnv.Auth__Jwt__IdTokenSecret,
                                },
                                {
                                    name: 'Auth__Jwt__AccessTokenSecret',
                                    value: apiEnv.Auth__Jwt__AccessTokenSecret,
                                },
                                {
                                    name:
                                        'Auth__PerimeterSocialProviders__Github__ClientId',
                                    value:
                                        apiEnv.Auth__PerimeterSocialProviders__Github__ClientId,
                                },
                                {
                                    name:
                                        'Auth__PerimeterSocialProviders__Github__SecretKey',
                                    value:
                                        apiEnv.Auth__PerimeterSocialProviders__Github__SecretKey,
                                },
                                {
                                    name:
                                        'Auth__PerimeterSocialProviders__Google__ClientId',
                                    value:
                                        apiEnv.Auth__PerimeterSocialProviders__Google__ClientId,
                                },
                                {
                                    name:
                                        'Auth__PerimeterSocialProviders__Google__SecretKey',
                                    value:
                                        apiEnv.Auth__PerimeterSocialProviders__Google__SecretKey,
                                },
                                {
                                    name: 'SendGridApiKey',
                                    value: apiEnv.SendGridApiKey,
                                },
                            ],
                        },
                    ],
                },
            },
        },
    });
    return prrApiDeployment;
};
