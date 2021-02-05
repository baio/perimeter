import { createApi } from './api';
import { AppConfig } from './app-config';
import { createMongo } from './mongo';
import { create as createJaeger } from './observability/jaeger/create';
import { create as createSeq } from './observability/seq/create';
import { createPsql } from './psql';
import { create as createRabbit } from './rabbit/create';

export const createK8sCluster = (version: string, config: AppConfig) => {
    const api = createApi(version, config.api, config.registry);
    const psql = createPsql(config.psql);
    const mongo = createMongo(config.mongo);
    // const adminApp = createAdminApp(version, config.adminApp, config.registry);
    const rabbit = createRabbit();
    const jaeger = createJaeger();
    const seq = createSeq();
    // prometheus is not setup since it requires add whole persistent volume / claim story to config (insane shit)
    return {
        api,
        psql,
        mongo,
        jaeger,
        seq,
        rabbit,
        //adminApp,
    };
};
