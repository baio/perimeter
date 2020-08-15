import { ApisDataAccessService } from '@admin/data-access';
import { AdminForm } from '@admin/shared';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { definition } from './form.definition';

@Component({
    selector: 'admin-api-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class ApiFormComponent {
    private readonly domainId: number;
    readonly definition = definition;
    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => this.dataAccess.loadItem(this.domainId, id);

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) =>
        item.id
            ? this.dataAccess.updateItem(this.domainId, item.id, item)
            : this.dataAccess.createItem(this.domainId, item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: ApisDataAccessService,
        private readonly router: Router
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
    }
}
