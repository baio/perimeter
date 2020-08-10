import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import { FormComponent as DomainsFormComponent } from './domains/form/form.component';
import { ListComponent as DomainsListComponent } from './domains/list/list.component';
import { MainLayoutComponent, AdminSharedModule } from '@admin/shared';

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
    declarations: [DomainsListComponent, DomainsFormComponent],
    imports: [AdminSharedModule, RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class AdminDomainsRoutingModule {}
