export interface LogIn {
    client_id: string;
    response_type: 'code';
    state?: string;
    redirect_uri: string;
    scopes: string[];
}
