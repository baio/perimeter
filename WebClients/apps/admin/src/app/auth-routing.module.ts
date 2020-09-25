import {
    IPPagesModule,
    LoginPageComponent,
    SignupPageComponent,
    SignupConfirmSentPageComponent,
    SignupConfirmPageComponent,
    ForgotPasswordPageComponent,
    ForgotPasswordSentPageComponent,
    ForgotPasswordResetPageComponent,
} from '@ip/auth';
import {
    Routes,
    RouterModule,
    Router,
    NavigationCancel,
} from '@angular/router';
import { NgModule } from '@angular/core';
import { filter } from 'rxjs/operators';
import { ROUTE_NOT_AUTHENTICATED } from 'libs/admin/domains/src/lib/client.guard';

// Exclusively for cypress tests !
// Multi host apps is critical for test with oauth protocol, cypress has poor support for these scenarios
// https://github.com/cypress-io/cypress/issues/461
// So we move all auth idp pages / routings under the same app
// TODO : Should be disabled during regular builds !!!

export const routes: Routes = [
    {
        path: 'auth',
        children: [
            { path: 'login', component: LoginPageComponent },
            { path: 'register', component: SignupPageComponent },
            { path: 'register-confirm', component: SignupConfirmPageComponent },
            {
                path: 'register-sent',
                component: SignupConfirmSentPageComponent,
            },
            { path: 'forgot-password', component: ForgotPasswordPageComponent },
            {
                path: 'forgot-password-sent',
                component: ForgotPasswordSentPageComponent,
            },
            {
                path: 'forgot-password-reset',
                component: ForgotPasswordResetPageComponent,
            },
        ],
    },
]; // sets up routes constant where you define your routes

@NgModule({
    imports: [IPPagesModule, RouterModule.forChild(routes)],
})
export class AuthRoutingModule {
    constructor(router: Router) {
        router.events
            .pipe(filter((event) => event instanceof NavigationCancel))
            .subscribe((event: NavigationCancel) => {
                switch (event.reason) {
                    case ROUTE_NOT_AUTHENTICATED:
                        // navigate to login page
                        router.navigate(['/auth/login']);
                        break;
                }
            });
    }
}
