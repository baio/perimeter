import {
    AdminsDataAccessService,
    TenantAdminsDataAccessService,
} from '@admin/data-access';
import { AdminForm, isNew$ } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { getDefinition } from './form.definition';

@Component({
    selector: 'admin-tenant-admin-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class TenantAdminFormComponent {
    private readonly tenantId = +this.activatedRoute.parent.parent.snapshot.params['id'];
    readonly definition: AdminForm.FormDefinition;

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: string
    ) => this.dataAccess.loadItem(this.tenantId, id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => this.dataAccess.updateItem(this.tenantId, item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: TenantAdminsDataAccessService
    ) {
        this.definition = getDefinition(
            isNew$(activatedRoute),
            this.dataAccess.getAllRoles()
        );
    }
}
