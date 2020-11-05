import { authenticate } from '@admin/profile';
import { NgModule } from '@angular/core';
import { NavigationStart, Router, RouterModule, Routes } from '@angular/router'; // CLI imports router
import { Store } from '@ngrx/store';
import { filter, map, skipWhile, take } from 'rxjs/operators';
import { BlankPageComponent } from './pages/blank-page/blank-page.component';
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
    constructor(router: Router, store: Store) {
        router.events
            .pipe(
                filter((f) => f instanceof NavigationStart),
                map((x: NavigationStart) => x.url),
                skipWhile(
                    (url) =>
                        url.startsWith('/login-cb') ||
                        url.startsWith('/home') ||
                        url.startsWith('/auth')
                ),
                take(1)
            )
            .subscribe(() => {
                store.dispatch(authenticate());
            });
    }
}
