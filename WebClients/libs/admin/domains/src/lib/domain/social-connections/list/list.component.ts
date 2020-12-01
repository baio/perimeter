import {
    SocialConnection,
    SocialConnectionsDataAccessService,
} from '@admin/data-access';
import { AdminListComponent } from '@admin/shared';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HlcNzTable, RowDropEvent } from '@nz-holistic/nz-list';
import { fromPairs, orderBy, pipe } from 'lodash/fp';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, withLatestFrom } from 'rxjs/operators';
import { listDefinition } from './list.definition';

const filterData = (state: HlcNzTable.Data.DataProviderState) => (
    data: SocialConnection[]
) => {
    if (state.filter) {
        if (state.filter.text) {
            return data.filter((f) => f.name.indexOf(state.filter.text) !== -1);
        } else if (state.filter.onlyEnabled) {
            return data.filter((f) => f.isEnabled);
        }
    }
    return data;
};

const sortData = (
    isOrderMode: boolean,
    state: HlcNzTable.Data.DataProviderState
) => (data: SocialConnection[]) => {
    if (isOrderMode) {
        return orderBy('order', 'asc', data);
    } else if (state.sort) {
        return orderBy(
            state.sort.key,
            state.sort.order === 'ascend' ? 'asc' : 'desc',
            data
        );
    } else {
        return data;
    }
};

const sortAndFilterData = (
    isOrderMode,
    data: SocialConnection[],
    state: HlcNzTable.Data.DataProviderState
) => pipe(filterData(state), sortData(isOrderMode, state))(data);

export interface View {
    sortMode: boolean;
    viewOptions: HlcNzTable.ViewOptions;
}

@Component({
    selector: 'admin-social-connections-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
})
export class SocialConnectionsListComponent implements OnInit {
    readonly view$: Observable<View>;
    private readonly sortMode = new BehaviorSubject(false);
    private readonly domainId: number;
    readonly listDefinition = listDefinition;

    @ViewChild(AdminListComponent) listComponent: AdminListComponent;

    readonly dataProvider: HlcNzTable.Data.DataProvider = (state) => {
        // load data only initially, then reuse them
        return this.dataAccess.loadList(this.domainId, state).pipe(
            withLatestFrom(this.sortMode),
            map(([data, sortMode]) => ({
                data: sortAndFilterData(sortMode, data, state),
                pager: {
                    size: data.length,
                    total: data.length,
                    index: 1,
                },
            }))
        );
    };

    constructor(
        activatedRoute: ActivatedRoute,
        private readonly dataAccess: SocialConnectionsDataAccessService
    ) {
        this.domainId = +activatedRoute.parent.snapshot.params['id'];
        this.view$ = this.sortMode.pipe(
            map((sortMode) => ({
                sortMode,
                viewOptions: {
                    hidePager: true,
                    hideSort: sortMode,
                },
            }))
        );
    }

    ngOnInit(): void {}

    onSortModeChanged(f: boolean) {
        this.sortMode.next(f);
        if (f) {
            this.listComponent.setFilter({ onlyEnabled: true });
        } else {
            this.listComponent.setFilter({ onlyEnabled: false });
        }
    }

    async onRowDrop(event: RowDropEvent) {
        const order = fromPairs(
            event.currentRows.map((m: SocialConnection, i) => [m.name, i])
        );

        await this.dataAccess.order(this.domainId, order).toPromise();
    }
}
