import { Component, OnInit } from '@angular/core';
import { definition } from './form.definition';
import { AdminForm } from '@admin/shared';
import { ActivatedRoute, Router } from '@angular/router';
import { DomainsDataAccessService } from '@admin/data-access';
import { of } from 'rxjs';

@Component({
    selector: 'admin-domain-pool-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class DomainPoolFormComponent {
    readonly definition = definition;

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => item.id ? this.dataAccess.updateItem(item.id, item) : this.dataAccess.createItem(item);

    readonly loadValueDataAccess: AdminForm.Data.LoadValueDataAccess = (
        id: number
    ) => id ? this.dataAccess.loadItem(id) : of({});

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: DomainsDataAccessService,
        private readonly router: Router
    ) {}
}
