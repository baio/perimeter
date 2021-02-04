import { createAdminApp } from './admin-app';
import { createApi } from './api';
import { AppConfig } from './app-config';
import { createMongo } from './mongo';
import { createPsql } from './psql';
import { create as createJaeger } from './observability/jaeger/create';

export const createK8sCluster = (version: string, config: AppConfig) => {
    const api = createApi(version, config.api, config.registry);
    const psql = createPsql(config.psql);
    const mongo = createMongo(config.mongo);
    // const adminApp = createAdminApp(version, config.adminApp, config.registry);
    const jaeger = createJaeger();
    return {
        api,
        psql,
        mongo,
        jaeger,
        //adminApp,
    };
};
