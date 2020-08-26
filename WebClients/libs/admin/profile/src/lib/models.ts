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

export interface ProfileState {
    user: User;
    domains: { [key: number]: Domain };
}
