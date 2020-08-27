export interface User {
    name: string;
}

export interface Item {
    id: number;
    name: string;
}

export interface Domain {
    id: number;
    tenant: Item;
    poolName: string;
    envName: string;
    managementClientId: string;
    isTenantManagement: boolean;
    roles: Item[];
}

export type ProfileStatus =
    | 'init'
    | 'notAuthenticated'
    | 'authenticating'
    | 'success'
    | 'error';

export interface ProfileState {
    status: ProfileStatus;
    user: User;
    domains: { [key: number]: Domain };
}
