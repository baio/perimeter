export const environment = {
    production: true,
    baseUrl: 'https://perimeter.azurefd.net/api',
    auth: {
        loginUrl: 'https://perimeter.azurefd.net/auth/login',
        signupUrl: 'https://perimeter.azurefd.net/auth/register',
        tokenUrl: 'https://perimeter.azurefd.net/api/auth/token',
        logoutUrl: 'https://perimeter.azurefd.net/api/auth/logout',
        returnLoginUri: 'https://perimeter.azurefd.net/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: 'https://perimeter.azurefd.net',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid email profile offline_access',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,
        refreshTokenUrl: 'https://perimeter.azurefd.net/api/auth/token',
    },
};
