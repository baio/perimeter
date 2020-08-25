import { NgModule } from '@angular/core';
import { NavigationStart, Router, RouterModule, Routes } from '@angular/router'; // CLI imports router
import { AuthService } from '@perimeter/ngx-auth';
import { isNil } from 'lodash/fp';
import { filter } from 'rxjs/operators';
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
        pathMatch: 'full',
    },
]; // sets up routes constant where you define your routes

const getDomainIdFromUrl = (url: string) => {
    if (/^\/tenant\//.test(url)) {
        return -1;
    } else if (/^\/domains\/(\d+)/.test(url)) {
        const m = url.match(/^\/domains\/(\d+)/);
        return +m[1];
    } else {
        return null;
    }
};

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
export class AppRoutingModule {
    constructor(router: Router, authService: AuthService) {
        // check if domain was changed by url on navigation start
        // 1. $\/tenant\/ - 0 domain
        // 2. $\/domains\/:id - id domain
        // If they was changed relogin (login user under sso)
        router.events
            .pipe(filter((f) => f instanceof NavigationStart))
            .subscribe((x: NavigationStart) => {
                const url = x.url;
                const domainId = getDomainIdFromUrl(url);
                console.log('+++', domainId);
            });
    }
}
