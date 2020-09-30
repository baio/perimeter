import { MainLayoutComponent, MainLayoutModule } from '@admin/shared';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router'; // CLI imports router
import { ClientGuard } from './client.guard';
import { AdminActivitiesListComponent } from './domain/admin-activities/list/list.component';
import { AdminFormComponent } from './domain/admins/form/form.component';
import { AdminsListComponent } from './domain/admins/list/list.component';
import { ApiFormComponent } from './domain/apis/form/form.component';
import { ApisListComponent } from './domain/apis/list/list.component';
import { AppFormComponent } from './domain/apps/form/form.component';
import { AppsListComponent } from './domain/apps/list/list.component';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';
import { DomainFormComponent } from './domain/info/form/form.component';
import { PermFormComponent } from './domain/perms/form/form.component';
import { PermsListComponent } from './domain/perms/list/list.component';
import { RoleFormComponent } from './domain/roles/form/form.component';
import { RolesListComponent } from './domain/roles/list/list.component';
import { UserActivitiesListComponent } from './domain/user-activities/list/list.component';
import { UserFormComponent } from './domain/users/form/form.component';
import { UsersListComponent } from './domain/users/list/list.component';
import { CreateTenantFormComponent } from './profile/create-tenant-form/create-tenant-form.component';
import { ProfileHomeComponent } from './profile/profile-home/profile-home.component';
import { ProfileLayoutComponent } from './profile/profile-layout/profile-layout.component';
import { CreateEnvFormComponent } from './tenant/domains/create-env-form/create-env-form.component';
import { DomainPoolFormComponent } from './tenant/domains/form/form.component';
import { DomainsPoolListComponent } from './tenant/domains/list/list.component';
import { TenantAdminFormComponent } from './tenant/tenant-admins/form/form.component';
import { TenantAdminsListComponent } from './tenant/tenant-admins/list/list.component';
import { TenantLayoutComponent } from './tenant/tenant-layout/tenant-layout.component';

const routes: Routes = [
    {
        path: '',
        component: MainLayoutComponent,
        children: [
            {
                path: 'profile',
                component: ProfileLayoutComponent,
                children: [
                    {
                        path: 'home',
                        component: ProfileHomeComponent,
                        children: [
                            {
                                path: 'create-tenant',
                                component: CreateTenantFormComponent,
                            },
                        ],
                    },
                ],
            },
            {
                path: 'domains/:id',
                component: DomainLayoutComponent,
                canActivateChild: [ClientGuard],
                children: [
                    {
                        path: 'info',
                        component: DomainFormComponent,
                    },
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
                    {
                        path: 'user-activities',
                        component: UserActivitiesListComponent,
                    },
                    {
                        path: 'admin-activities',
                        component: AdminActivitiesListComponent,
                    },
                ],
            },
            {
                path: 'tenants/:id',
                component: TenantLayoutComponent,
                canActivateChild: [ClientGuard],
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
    providers: [ClientGuard],
})
export class AdminDomainsRoutingModule {}
