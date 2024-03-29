import { Component, OnInit } from '@angular/core';
import { getDefinition } from './form.definition';
import { AdminForm, isNew$ } from '@admin/shared';
import { ActivatedRoute, Router } from '@angular/router';
import { AppsDataAccessService } from '@admin/data-access';

@Component({
    selector: 'admin-apps-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class AppFormComponent {
    private readonly domainId: number;
    readonly definition = getDefinition(isNew$(this.activatedRoute));
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
        private readonly dataAccess: AppsDataAccessService,
        private readonly router: Router
    ) {
        this.domainId = +activatedRoute.parent.parent.snapshot.params['id'];
    }
}
