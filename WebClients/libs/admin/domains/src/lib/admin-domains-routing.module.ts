import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import { MainLayoutComponent, MainLayoutModule } from '@admin/shared';
import { ListComponent as DomainsListComponent } from './domains/list/list.component';

const routes: Routes = [
    {
        path: 'domains',
        component: MainLayoutComponent,
        children: [
            {
                path: '',
                component: DomainsListComponent,
            },
        ],
    },
]; // sets up routes constant where you define your routes

// configures NgModule imports and exports
@NgModule({
    declarations: [],
    imports: [MainLayoutModule, RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class AdminDomainsRoutingModule {}
