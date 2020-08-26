import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router'; // CLI imports router
import { MainLayoutComponent, MainLayoutModule } from '@admin/shared';
import { DomainsPoolListComponent } from './tenant/domains/list/list.component';
import { AppsListComponent } from './domain/apps/list/list.component';
import { TenantLayoutComponent } from './tenant/tenant-layout/tenant-layout.component';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';
import { AppFormComponent } from './domain/apps/form/form.component';
import { ApiFormComponent } from './domain/apis/form/form.component';
import { ApisListComponent } from './domain/apis/list/list.component';
import { PermsListComponent } from './domain/perms/list/list.component';
import { PermFormComponent } from './domain/perms/form/form.component';
import { RolesListComponent } from './domain/roles/list/list.component';
import { RoleFormComponent } from './domain/roles/form/form.component';
import { UsersListComponent } from './domain/users/list/list.component';
import { UserFormComponent } from './domain/users/form/form.component';
import { AdminsListComponent } from './domain/admins/list/list.component';
import { AdminFormComponent } from './domain/admins/form/form.component';
import { DomainPoolFormComponent } from './tenant/domains/form/form.component';
import { CreateEnvFormComponent } from './tenant/domains/create-env-form/create-env-form.component';
import { TenantAdminsListComponent } from './tenant/tenant-admins/list/list.component';
import { TenantAdminFormComponent } from './tenant/tenant-admins/form/form.component';

const routes: Routes = [
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            {
                path: 'domains/:id',
                component: DomainLayoutComponent,
                children: [
                    {
                        path: 'apps',
                        component: AppsListComponent,
                        children: [
                            {
                                path: ':id',
                                component: AppFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'apis/:id/permissions',
                        component: PermsListComponent,
                        children: [
                            {
                                path: ':id',
                                component: PermFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'apis',
                        component: ApisListComponent,
                        children: [
                            {
                                path: ':id',
                                component: ApiFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'roles',
                        component: RolesListComponent,
                        children: [
                            {
                                path: ':id',
                                component: RoleFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'users',
                        component: UsersListComponent,
                        children: [
                            {
                                path: ':id',
                                component: UserFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'admins',
                        component: AdminsListComponent,
                        children: [
                            {
                                path: ':id',
                                component: AdminFormComponent,
                            },
                        ],
                    },
                ],
            },
            {
                path: 'tenants/:id',
                component: TenantLayoutComponent,
                children: [
                    {
                        path: 'domains',
                        component: DomainsPoolListComponent,
                        children: [
                            {
                                path: ':id/new-env',
                                component: CreateEnvFormComponent,
                            },
                            {
                                path: ':id',
                                component: DomainPoolFormComponent,
                            },
                        ],
                    },
                    {
                        path: 'admins',
                        component: TenantAdminsListComponent,
                        children: [
                            {
                                path: ':id',
                                component: TenantAdminFormComponent,
                            },
                        ],
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
