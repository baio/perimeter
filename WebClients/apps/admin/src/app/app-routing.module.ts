import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router'; // CLI imports router
import { HomePageComponent } from './pages/home-page/home-page.component';
import { LoginCbPageComponent } from './pages/login-cb-page/login-cb-page.component';
import { AdminPagesModule } from './pages/pages.module';

const routes: Routes = [
    {
        path: 'login-cb',
        component: LoginCbPageComponent,
    },
    {
        path: 'home',
        component: HomePageComponent,
    },
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    },
]; // sets up routes constant where you define your routes

// configures NgModule imports and exports
@NgModule({
    imports: [
        AdminPagesModule,
        RouterModule.forRoot(routes, {
            enableTracing: false,
            initialNavigation: true,
        }),
    ],
    exports: [RouterModule],
})
export class AppRoutingModule {}
