import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import {
    IPPagesModule,
    LoginPageComponent,
    SignupPageComponent,
    SignupConfirmSentPageComponent,
    SignupConfirmPageComponent,
    ForgotPasswordPageComponent,
    ForgotPasswordSentPageComponent,
    ForgotPasswordResetPageComponent,
} from '@idp/auth';

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
    {
        path: '**',
        redirectTo: 'auth/login',
    },
]; // sets up routes constant where you define your routes

// configures NgModule imports and exports
@NgModule({
    imports: [
        RouterModule.forRoot(routes, { enableTracing: false }),
        IPPagesModule,
    ],
    exports: [RouterModule],
})
export class AppRoutingModule {}
