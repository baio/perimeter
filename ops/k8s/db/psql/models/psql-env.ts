import { Output } from "@pulumi/pulumi";

export interface PsqlConfig {
    POSTGRES_DB: string;
    POSTGRES_USER: string;
    POSTGRES_PASSWORD: Output<string>;
    storageSize: number;
    dataPath: string;
}
