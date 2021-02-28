export interface AppItem {
    id: number;
    name: string;
    clientId: string;
    idTokenExpiresIn: number;
    refreshTokenExpiresIn: number;
    dateCreated: string;
    grantTypes: string[];
}
