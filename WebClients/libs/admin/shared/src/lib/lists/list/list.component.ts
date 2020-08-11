import {
    AfterViewInit,
    Component,
    ContentChild,
    EventEmitter,
    Input,
    OnDestroy,
    OnInit,
    Output,
    ViewChild,
    Optional,
    SkipSelf,
    Inject,
    ContentChildren,
    QueryList,
    forwardRef,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminListService } from '../../common';
import {
    ActionClickEvent,
    HlcNzTable,
    HlcNzTableComponent,
    HlcNzTableFilterService,
    HLC_NZ_TABLE_FILTER_VALUE_CHANGE_DELAY,
    RowClickEvent,
    RowDropEvent,
    HlcNzTableCustomCellsProvider,
    HLC_NZ_TABLE_CUSTOM_CELLS_PROVIDER,
    HlcNzCustomCellDirective,
} from '@nz-holistic/nz-list';
import { NzModalService } from 'ng-zorro-antd';
import { merge, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AdminListHeaderComponent } from '../list-header/list-header.component';
import { AdminList } from './list.models';
import {
    addDefinitionDeleteButtonAction,
    addDefinitionLinkButtonAction,
    DELETE_ACTION_ID,
} from './utils';
import { concat } from 'lodash/fp';

@Component({
    selector: 'admin-list',
    templateUrl: './list.component.html',
    styleUrls: ['./list.component.scss'],
    providers: [
        { provide: HLC_NZ_TABLE_FILTER_VALUE_CHANGE_DELAY, useValue: 0 },
        {
            provide: HLC_NZ_TABLE_CUSTOM_CELLS_PROVIDER,
            useExisting: forwardRef(() => AdminListComponent),
        },
        HlcNzTableFilterService,
        AdminListService,
    ],
})
export class AdminListComponent
    implements HlcNzTableCustomCellsProvider, OnInit, OnDestroy, AfterViewInit {
    @Input()
    canAdd = true;

    @Input()
    dataProvider: HlcNzTable.Data.DataProvider;

    @Input()
    definition: AdminList.TableDefinition;

    @Input()
    title: string;

    @Input()
    removeItemDataAccess: AdminList.Data.RemoveItemDataAccess | undefined;

    @Output()
    actionClick = new EventEmitter<ActionClickEvent>();

    @Output()
    rowClick = new EventEmitter<RowClickEvent>();

    @ContentChild(AdminListHeaderComponent, { static: false })
    header: AdminListHeaderComponent;

    @ViewChild(HlcNzTableComponent, { static: false })
    hlcList: HlcNzTableComponent;

    private readonly destroy$ = new Subject();
    hlcDefinition: HlcNzTable.TableDefinition;

    viewOptions: HlcNzTable.ViewOptions = {
        hidePager: false,
        hideSort: false,
    };

    /**
     * Custom cells
     */
    @ContentChildren(HlcNzCustomCellDirective) customCellsContent: QueryList<
        HlcNzCustomCellDirective
    >;

    constructor(
        private modalService: NzModalService,
        private readonly activatedRoute: ActivatedRoute,
        private readonly listService: AdminListService,
        private readonly router: Router,
        @Optional()
        @SkipSelf()
        @Inject(HLC_NZ_TABLE_CUSTOM_CELLS_PROVIDER)
        private readonly containerCustomCellsProvider?: HlcNzTableCustomCellsProvider
    ) {}

    ngOnInit() {
        this.hlcDefinition = this.definition;

        this.hlcDefinition = this.hlcDefinition.layout
            ? this.hlcDefinition
            : { ...this.hlcDefinition };

        if (this.definition.hasLinkButton) {
            const linkButtonTitle =
                typeof this.definition.hasLinkButton === 'string'
                    ? this.definition.hasLinkButton
                    : null;

            this.hlcDefinition = addDefinitionLinkButtonAction(
                linkButtonTitle,
                this.hlcDefinition
            );
        }

        if (this.removeItemDataAccess) {
            this.hlcDefinition = addDefinitionDeleteButtonAction(
                this.hlcDefinition
            );
        }

        merge(
            this.listService.rowAdded,
            this.listService.rowUpdated,
            this.listService.rowRemoved
        )
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                this.hlcList.reload();
            });
    }

    ngAfterViewInit() {}

    ngOnDestroy() {
        this.destroy$.next();
    }

    onRowClick($event: RowClickEvent) {
        this.rowClick.emit($event);
        this.router.navigate(['.', $event.row.id], {
            relativeTo: this.activatedRoute,
        });
    }

    async onRowDrop($event: RowDropEvent) {}

    onActionClick(action: ActionClickEvent) {
        this.actionClick.emit(action);

        if (action.actionId === DELETE_ACTION_ID) {
            this.modalService.confirm({
                nzTitle: 'Do you want to delete this item?',
                nzContent:
                    "You are about to delete item, you can't restore it later.",
                nzOkText: 'Yes',
                nzCancelText: 'No',
                nzOnOk: async () => {
                    // TODO : Suspense
                    await this.removeItemDataAccess(action.row).toPromise();
                    this.reload();
                    return true;
                },
            });
        }
    }

    private reload() {
        this.hlcList.reload();
    }

    get customCells() {
        return concat(
            this.customCellsContent ? this.customCellsContent.toArray() : [],
            (this.containerCustomCellsProvider &&
                this.containerCustomCellsProvider.customCells) ||
                []
        );
    }
}
