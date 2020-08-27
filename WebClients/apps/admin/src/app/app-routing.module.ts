import { NgModule } from '@angular/core';
import { NavigationStart, Router, RouterModule, Routes } from '@angular/router'; // CLI imports router
import { AuthService } from '@perimeter/ngx-auth';
import { isNil } from 'lodash/fp';
import { filter, map, withLatestFrom } from 'rxjs/operators';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { LoginCbPageComponent } from './pages/login-cb-page/login-cb-page.component';
import { AdminPagesModule } from './pages/pages.module';
import { BlankPageComponent } from './pages/blank-page/blank-page.component';
import { Store } from '@ngrx/store';
import { selectProfileDomainsList } from '@admin/profile';
import { combineLatest } from 'rxjs';

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
        component: BlankPageComponent,
        pathMatch: 'full',
    },
]; // sets up routes constant where you define your routes


// configures NgModule imports and exports
@NgModule({
    imports: [
        AdminPagesModule,
        RouterModule.forRoot(routes, {
            enableTracing: true,
            initialNavigation: true,
        }),
    ],
    exports: [RouterModule],
})
export class AppRoutingModule {
    constructor(router: Router, authService: AuthService, store: Store) {
        /*
        // check if domain was changed by url on navigation start
        // 1. $\/tenants\/:id - tenant id
        // 2. $\/domains\/:id - domain id
        // If they was changed relogin (login user under sso)
        const urlObj$ = router.events.pipe(
            filter((f) => f instanceof NavigationStart),
            map(({ url }: NavigationStart) => getObjIdFromUrl(url)),
            filter((obj) => !!obj)
        );

        const domains$ = store.select(selectProfileDomainsList);

        combineLatest([urlObj$, domains$]).subscribe(([obj, domains]) => {
            const idToken = authService.validateTokens();
            console.log('+++', obj, idToken, domains);
        });
        */
    }
}
