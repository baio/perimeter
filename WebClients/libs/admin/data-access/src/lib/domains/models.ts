export interface DomainEnv {
    id: number;
    name: string;
    isMain: boolean;
}

export interface DomainItem {
    id: number;
    name: string;
    envs: DomainEnv[];
    dateCreated: string;
}
