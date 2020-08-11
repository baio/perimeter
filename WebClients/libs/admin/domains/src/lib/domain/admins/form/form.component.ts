import { ApisDataAccessService, AdminsDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { definition } from './form.definition';

@Component({
    selector: 'admin-admin-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class AdminFormComponent {
    readonly definition = definition;

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => this.dataAccess.loadItem(id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        item.id
            ? this.dataAccess.updateItem(item.id, item)
            : this.dataAccess.createItem(item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: AdminsDataAccessService,
        private readonly router: Router
    ) {}
}
