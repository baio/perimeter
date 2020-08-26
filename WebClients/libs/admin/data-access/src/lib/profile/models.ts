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
