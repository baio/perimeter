import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDomainsRoutingModule } from './admin-domains-routing.module';
import {
    AdminListModule,
    AdminListHeaderModule,
    AdminFormModule,
} from '@admin/shared';
import { FormComponent as DomainsFormComponent } from './pool/domains/form/form.component';
import { DomainsListComponent } from './pool/domains/list/list.component';
import { AppsListComponent } from './domain/apps/list/list.component';

import { DataAccessModule } from '@admin/data-access';
import {
    NzButtonModule,
    NzIconModule,
    NzLayoutModule,
    NzBreadCrumbModule,
    NzMenuModule,
} from 'ng-zorro-antd';
import { DomainLayoutComponent } from './domain/domain-layout/domain-layout.component';
import { PoolLayoutComponent } from './pool/pool-layout/pool-layout.component';
import { AppFormComponent } from './domain/apps/form/form.component';
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
    ],
    declarations: [
        DomainLayoutComponent,
        PoolLayoutComponent,
        AppsListComponent,
        DomainsListComponent,
        DomainsFormComponent,
        AppFormComponent,
    ],
})
export class AdminDomainsModule {}
