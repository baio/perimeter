import { Output } from "@pulumi/pulumi";

export interface MongoConfig {
    MONGO_USER: string;
    MONGO_PASSWORD: Output<string>;
    storageSize: number;
    dataPath: string;
}
