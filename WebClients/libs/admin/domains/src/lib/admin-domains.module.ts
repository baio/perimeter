import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDomainsRoutingModule } from './admin-domains-routing.module';
import {
    AdminListModule,
    AdminListHeaderModule,
    AdminFormModule,
} from '@admin/shared';
import { DomainPoolFormComponent } from './pool/domains/form/form.component';
import { DomainsPoolListComponent } from './pool/domains/list/list.component';
import { AppsListComponent } from './domain/apps/list/list.component';

import { DataAccessModule } from '@admin/data-access';
import {
    NzButtonModule,
    NzIconModule,
    NzLayoutModule,
    NzBreadCrumbModule,
    NzMenuModule,
    NzDividerModule,
} from 'ng-zorro-antd';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';
import { PoolLayoutComponent } from './pool/pool-layout/pool-layout.component';
import { AppFormComponent } from './domain/apps/form/form.component';
import { ApisListComponent } from './domain/apis/list/list.component';
import { ApiFormComponent } from './domain/apis/form/form.component';
import { AdminsListComponent } from './domain/admins/list/list.component';
import { AdminFormComponent } from './domain/admins/form/form.component';
import { PermsListComponent } from './domain/perms/list/list.component';
import { PermFormComponent } from './domain/perms/form/form.component';
import { RolesListComponent } from './domain/roles/list/list.component';
import { RoleFormComponent } from './domain/roles/form/form.component';
import { UsersListComponent } from './domain/users/list/list.component';
import { UserFormComponent } from './domain/users/form/form.component';
import { CreateEnvFormComponent } from './pool/domains/create-env-form/create-env-form.component';
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
        PoolLayoutComponent,
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
    ],
})
export class AdminDomainsModule {}
