export interface TokensResult {
    access_token: string;
    id_token: string;
    refresh_token: string;
}

export interface LoginResultOk {
    kind: 'ok';
    code: string;
    state: string;
}

export type LoginResultErrorType =
    | 'code_not_found'
    | 'invalid_request'
    | 'access_denied'
    | 'unauthorized_client'
    | 'unsupported_response_type'
    | 'invalid_scope'
    | 'server_error'
    | 'temporarily_unavailable';

export interface LoginResultError {
    kind: 'error';
    error: LoginResultErrorType;
    description?: string;
    uri?: string;
}

export type LoginResult = LoginResultOk | LoginResultError;
