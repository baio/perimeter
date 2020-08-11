import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import { MainLayoutComponent, MainLayoutModule } from '@admin/shared';
import { DomainsListComponent } from './pool/domains/list/list.component';
import { AppsListComponent } from './domain/apps/list/list.component';
import { PoolLayoutComponent } from './pool/pool-layout/pool-layout.component';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';

const routes: Routes = [
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            {
                path: 'domains',
                component: PoolLayoutComponent,
                children: [
                    {
                        path: 'pool',
                        component: DomainsListComponent,
                    },
                ],
            },
            {
                path: 'domains/:id',
                component: DomainLayoutComponent,
                children: [
                    {
                        path: 'apps',
                        component: AppsListComponent,
                    },
                ],
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
