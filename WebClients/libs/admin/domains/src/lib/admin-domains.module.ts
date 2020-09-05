import { DataAccessModule } from '@admin/data-access';
import {
    AdminFormModule,
    AdminListHeaderModule,
    AdminListModule,
} from '@admin/shared';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import {
    NzBreadCrumbModule,
    NzButtonModule,
    NzDividerModule,
    NzIconModule,
    NzLayoutModule,
    NzMenuModule,
} from 'ng-zorro-antd';
import { AdminDomainsRoutingModule } from './admin-domains-routing.module';
import { AdminFormComponent } from './domain/admins/form/form.component';
import { AdminsListComponent } from './domain/admins/list/list.component';
import { ApiFormComponent } from './domain/apis/form/form.component';
import { ApisListComponent } from './domain/apis/list/list.component';
import { AppFormComponent } from './domain/apps/form/form.component';
import { AppsListComponent } from './domain/apps/list/list.component';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';
import { PermFormComponent } from './domain/perms/form/form.component';
import { PermsListComponent } from './domain/perms/list/list.component';
import { RoleFormComponent } from './domain/roles/form/form.component';
import { RolesListComponent } from './domain/roles/list/list.component';
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

@NgModule({
    imports: [
        CommonModule,
        AdminDomainsRoutingModule,
        AdminFormModule,
        AdminListModule,
        AdminListHeaderModule,
        DataAccessModule,
        DataAccessModule,
        NzButtonModule,
        NzIconModule,
        NzLayoutModule,
        NzBreadCrumbModule,
        NzMenuModule,
        NzDividerModule,
    ],
    declarations: [
        DomainLayoutComponent,
        TenantLayoutComponent,
        AppsListComponent,
        DomainsPoolListComponent,
        DomainPoolFormComponent,
        AppFormComponent,
        ApisListComponent,
        ApiFormComponent,
        AdminsListComponent,
        AdminFormComponent,
        PermsListComponent,
        PermFormComponent,
        RolesListComponent,
        RoleFormComponent,
        UsersListComponent,
        UserFormComponent,
        CreateEnvFormComponent,
        TenantAdminsListComponent,
        TenantAdminFormComponent,
        ProfileLayoutComponent,
        ProfileHomeComponent,
        CreateTenantFormComponent,
    ],
})
export class AdminDomainsModule {}
