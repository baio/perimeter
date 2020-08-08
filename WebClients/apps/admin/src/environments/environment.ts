// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: false,
    baseUrl: 'https://localhost:5001',
    auth: {
        baseUrl: 'http://localhost:4201/auth',
        loginPath: 'login',
        tokenPath: 'token',
        logoutPath: 'logout',
        returnUri: 'http://localhost:4200/cb',
        clientId: '__DEFAULT_CLIENT_ID__',
        scope: 'open_id profile',
        stateStringLength: 64,
        pkceRandomStringLength: 10,
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
