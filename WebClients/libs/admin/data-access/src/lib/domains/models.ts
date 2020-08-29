// Domain

export interface DomainEnv {
    id: number;
    envName: string;
    isMain: boolean;
    domainManagementClientId: string;
}

export interface DomainItem {
    id: number;
    name: string;
    envs: DomainEnv[];
    dateCreated: string;
}
