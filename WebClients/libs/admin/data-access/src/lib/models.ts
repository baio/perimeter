import { RoleItem } from './roles/models';

export interface UserRole {
    id: number;
    name: string;
}

export interface User {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    roles: UserRole[];
}

export interface Permission {
    id: number;
    name: string;
    description: string;
}
