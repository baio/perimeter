import { ImageRegistry } from '@pulumi/docker';

export interface ConfigPorts {
    port: number;
    targetPort: number;
}

export interface Config {
    ports: ConfigPorts;
}
