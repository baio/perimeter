import { DataAccessModule } from '@admin/data-access';
import {
    AdminFormModule,
    AdminListHeaderModule,
    AdminListModule,
} from '@admin/shared';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NzBreadCrumbModule } from 'ng-zorro-antd/breadcrumb';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { AdminDomainsRoutingModule } from './admin-domains-routing.module';
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
import { SocialConnectionsFormComponent } from './domain/social-connections/form/form.component';
import { SocialConnectionsListComponent } from './domain/social-connections/list/list.component';
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
        NzSwitchModule,
        FormsModule,
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
        DomainFormComponent,
        AdminActivitiesListComponent,
        UserActivitiesListComponent,
        SocialConnectionsListComponent,
        SocialConnectionsFormComponent,
    ],
})
export class AdminDomainsModule {}
