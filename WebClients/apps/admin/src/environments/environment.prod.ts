export const environment = {
    production: true,
    baseUrl: 'api',
    auth: {
        loginUrl: 'http://localhost:8070/auth/login',
        signupUrl: 'http://localhost:8070/auth/register',
        tokenUrl: 'api/auth/token',
        logoutUrl: 'api/auth/logout',
        returnLoginUri: 'http://localhost:8071/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: 'http://localhost:8071',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid profile',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,
        refreshTokenUrl: 'api/auth/refresh-token',
    },
};
