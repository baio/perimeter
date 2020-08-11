import { Component, OnInit } from '@angular/core';
import { definition } from './form.definition';
import { AdminForm } from '@admin/shared';
import { ActivatedRoute, Router } from '@angular/router';
import { AppsDataAccessService } from '@admin/data-access';

@Component({
    selector: 'admin-apps-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class AppFormComponent {
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
        private readonly dataAccess: AppsDataAccessService,
        private readonly router: Router
    ) {}
}
