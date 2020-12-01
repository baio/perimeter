import { Permission } from '../models';

export interface SocialConnection {
    id: string;
    name: string;
    isEnabled: boolean;
    clientId: string;
    clientSecret: string;
    attributes: string[];
    permissions: string[];
    order?: number;
}
