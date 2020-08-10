import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDomainsRoutingModule } from './admin-domains-routing.module';
import {
    AdminListModule,
    AdminListHeaderModule,
    AdminFormModule,
} from '@admin/shared';
import { FormComponent as DomainsFormComponent } from './domains/form/form.component';
import { ListComponent as DomainsListComponent } from './domains/list/list.component';

import { DataAccessModule } from '@admin/data-access';
@NgModule({
    imports: [
        CommonModule,
        AdminDomainsRoutingModule,
        AdminFormModule,
        AdminListModule,
        AdminListHeaderModule,
        DataAccessModule,
        DataAccessModule,
    ],
    declarations: [DomainsListComponent, DomainsFormComponent],
})
export class AdminDomainsModule {}
