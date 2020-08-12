// Domain

export interface DomainEnv {
    id: number;
    envName: string;
    isMain: boolean;
}

export interface DomainItem {
    id: number;
    name: string;
    envs: DomainEnv[];
    dateCreated: string;
}
