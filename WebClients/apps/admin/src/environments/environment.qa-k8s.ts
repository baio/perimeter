// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: true,
    baseUrl: '/api',
    auth: {
        loginUrl: 'http://perimeter.pw/auth/login',
        signupUrl: 'http://perimeter.pw/auth/register',
        tokenUrl: 'http://perimeter.pw:70/api/auth/token',
        logoutUrl: 'http://perimeter.pw:70/api/auth/logout',
        refreshTokenUrl: 'http://perimeter.pw:70/api/auth/refresh-token',
        returnLoginUri: '/login-cb',
        returnLoginPath: '/',
        returnLogoutUri: '/',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'openid profile',
        stateStringLength: 64,
        pkceCodeVerifierLength: 128,        
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
