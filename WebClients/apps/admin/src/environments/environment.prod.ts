export const environment = {
    production: true,
    baseUrl: 'https://localhost:5001',
    auth: {
        loginUrl: 'http://localhost:4200/auth/login',
        signupUrl: 'http://localhost:4200/auth/register',
        tokenUrl: 'https://localhost:5001/auth/token',
        logoutUrl: 'https://localhost:5001/auth/logout',
        returnLoginUri: 'http://localhost:4201/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: 'http://localhost:4201',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid profile',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,
        refreshTokenUrl: `https://localhost:5001/auth/refresh-token`,
    },
};
