export const environment = {
    production: true,
    baseUrl: 'http://localhost:5000',
    auth: {
        loginUrl: 'http://localhost:8071/auth/login',
        signupUrl: 'http://localhost:8071/auth/register',
        tokenUrl: 'http://localhost:5000/auth/token',
        logoutUrl: 'http://localhost:5000/auth/logout',
        returnLoginUri: 'http://localhost:8071/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: 'http://localhost:8071',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid profile',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,
        refreshTokenUrl: 'http://localhost:5000/auth/refresh-token',
    },
};
