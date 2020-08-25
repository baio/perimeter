export const USER_ROLE_TENANT_OWNER_ID = -100;
export const USER_ROLE_DOMAIN_OWNER_ID = -400;

export interface UserRole {
    id: string;
    email: string;
    roles: { id: number; name: string }[];
}

export interface User {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    roles: {
        id: number;
        name: string;
    }[];
}

export interface Permission {
    id: number;
    name: string;
    description: string;
}
