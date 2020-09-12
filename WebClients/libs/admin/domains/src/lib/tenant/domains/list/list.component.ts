import { Component, OnInit } from '@angular/core';
import { listDefinition } from './list.definition';
import { HlcNzTable, ActionClickEvent } from '@nz-holistic/nz-list';
import { AdminList } from '@admin/shared';
import { DomainsDataAccessService, DomainEnv } from '@admin/data-access';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '@perimeter/ngx-auth';

@Component({
    selector: 'admin-domains-pool-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class DomainsPoolListComponent implements OnInit {
    private readonly tenantId = +this.activatedRoute.parent.snapshot.params['id'];
    readonly listDefinition = listDefinition;
    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) =>
        this.dataAccess.loadList(this.tenantId, state);
    readonly removeItemDataAccess: AdminList.Data.RemoveItemDataAccess = ({
        id,
    }) => this.dataAccess.removeItem(this.tenantId, id);

    constructor(
        private readonly dataAccess: DomainsDataAccessService,
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly authService: AuthService
    ) {}

    ngOnInit(): void {}

    onActionClick($event: ActionClickEvent) {
        if ($event.actionId === 'add-env') {
            this.router.navigate(['.', $event.row.id, 'new-env'], {
                relativeTo: this.activatedRoute,
            });
        }
    }

    async onEnvClicked(
        event: MouseEvent,
        { id, domainManagementClientId }: DomainEnv
    ) {
        event.preventDefault();
        event.stopPropagation();
        await this.authService.authorize({
            useSSO: true,
            clientId: domainManagementClientId,
            redirectPath: `/domains/${id}/apps`,
        });
    }
}
