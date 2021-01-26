// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: false,
    baseUrl: 'https://localhost:5001/api',
    auth: {
        loginUrl: 'http://localhost:4201/auth/login',
        signupUrl: 'http://localhost:4201/auth/register',
        tokenUrl: 'https://localhost:5001/api/auth/token',
        logoutUrl: 'https://localhost:5001/api/auth/logout',
        returnLoginUri: 'http://localhost:4201/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: 'http://localhost:4201',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid profile',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,
        refreshTokenUrl: `https://localhost:5001/api/auth/refresh-token`,
    },
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
