export interface LoginParams {
    client_id: string;
    response_type: 'code';
    state: string;
    redirect_uri: string;
    scope: string;
    code_challenge: string;
    code_challenge_method: 'S256';
}

export interface LoginData extends LoginParams {    
    email: string;
    password: string;
}
