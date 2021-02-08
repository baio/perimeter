import { createAdminApp } from './admin-app';
import { createIdpApp } from './idp-app';
import { createAuthApi, createTenantApi } from './api';
import { AppConfig } from './app-config';
import { createMongo } from './mongo';
import { create as createJaeger } from './observability/jaeger/create';
import { create as createSeq } from './observability/seq/create';
import { createPsql } from './psql';
import { create as createRabbit } from './rabbit/create';

export const createK8sCluster = (version: string, config: AppConfig) => {
    const authApi = createAuthApi(version, config.authApi, config.registry);
    const tenantApi = createTenantApi(
        version,
        config.tenantApi,
        config.registry,
    );
    const adminApp = createAdminApp(version, config.adminApp, config.registry);
    //const idpApp = createIdpApp(version, config.idpApp, config.registry);
    //
    const psql = createPsql(config.psql);
    const mongo = createMongo(config.mongo);
    //
    const rabbit = createRabbit();
    //
    const jaeger = createJaeger();
    const seq = createSeq();
    // prometheus is not setup since it requires add whole persistent volume / claim story to config (insane shit)
    return {
        // idpApp,
        authApi,
        tenantApi,
        psql,
        mongo,
        jaeger,
        seq,
        rabbit,
        adminApp
    };
};
