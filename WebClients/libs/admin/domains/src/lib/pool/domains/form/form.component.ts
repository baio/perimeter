import { Component, OnInit } from '@angular/core';
import { definition } from './form.definition';
import { AdminForm } from '@admin/shared';
import { ActivatedRoute, Router } from '@angular/router';
import { DomainsDataAccessService } from '@admin/data-access';

@Component({
    selector: 'admin-domain-pool-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class DomainPoolFormComponent {
    readonly definition = definition;

    readonly storeValueDataAccess: AdminForm.Data.StoreValueDataAccess = (
        item: any
    ) => this.dataAccess.createItem(item);

    constructor(
        private readonly activatedRoute: ActivatedRoute,
        private readonly dataAccess: DomainsDataAccessService,
        private readonly router: Router
    ) {}
}
