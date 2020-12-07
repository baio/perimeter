import { ApiEnv } from './api-env';

export interface ApiConfigPorts {
    port: number;
    targetPort: number;
}

export interface ApiConfig {
    ports: ApiConfigPorts;
    env: ApiEnv;
}
