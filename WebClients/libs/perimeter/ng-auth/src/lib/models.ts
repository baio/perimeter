export interface TokensResult {
    accessToken: string;
    idToken: string;
    refreshToken: string;
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
    | 'temporarily_unavailable'
    | 'login_required';

export interface LoginResultError {
    kind: 'error';
    error: LoginResultErrorType;
    description?: string;
    uri?: string;
}

export type LoginResult = LoginResultOk | LoginResultError;
