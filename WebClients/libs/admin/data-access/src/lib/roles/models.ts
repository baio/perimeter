import { Permission } from '../models';

export interface RoleItem {
    id: number;
    name: string;
    description: string;
    permissions: Permission[];
    dateCreated: string;
}
