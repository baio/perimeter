import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import {
    LoginPageComponent,
    SignupPageComponent,
    SignupConfirmSentPageComponent,
    SignupConfirmPageComponent,
} from '@admin/auth';

const routes: Routes = [
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
        ],
    },
    {
        path: '**',
        redirectTo: 'auth/login',
    },
]; // sets up routes constant where you define your routes

// configures NgModule imports and exports
@NgModule({
    imports: [RouterModule.forRoot(routes, { enableTracing: true })],
    exports: [RouterModule],
})
export class AppRoutingModule {}
