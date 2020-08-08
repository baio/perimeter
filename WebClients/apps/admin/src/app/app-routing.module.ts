import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import { AdminPagesModule } from './pages/pages.module';
import { HomePageComponent } from './pages/home-page/home-page.component';

const routes: Routes = [
    {
        path: '',
        component: HomePageComponent,
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
