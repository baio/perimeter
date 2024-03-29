import { Permission } from '../models';

export interface ApiItem {
    id: number;
    name: string;
    identifier: string;
    permissions: Permission[];
    dateCreated: string;
}
