import {
    AfterViewInit,
    Component,
    ContentChild,
    ContentChildren,
    EventEmitter,
    forwardRef,
    Inject,
    Input,
    OnDestroy,
    OnInit,
    Optional,
    Output,
    QueryList,
    SkipSelf,
    ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoService } from '@ngneat/transloco';
import {
    ActionClickEvent,
    CellClickEvent,
    FilterValue,
    HlcNzCustomCellDirective,
    HlcNzTable,
    HlcNzTableComponent,
    HlcNzTableCustomCellsProvider,
    HlcNzTableFilterService,
    HLC_NZ_TABLE_CUSTOM_CELLS_PROVIDER,
    HLC_NZ_TABLE_FILTER_VALUE_CHANGE_DELAY,
    RowClickEvent,
    RowDropEvent,
} from '@nz-holistic/nz-list';
import { concat } from 'lodash/fp';
import { NzModalService } from 'ng-zorro-antd/modal';
import { merge, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdminListService } from '../../common';
import { AdminListHeaderComponent } from '../list-header/list-header.component';
import { AdminList } from './list.models';
import {
    addDefinitionDeleteButtonAction,
    addDefinitionLinkButtonAction,
    DELETE_ACTION_ID,
} from './utils';
import { addDefinitionTranslations } from './utils/add-definition-translations';

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
    errorMessage: string;

    @Input()
    canAdd = true;

    @Input()
    dataProvider: HlcNzTable.Data.DataProvider;

    @Input()
    definition: AdminList.TableDefinition;

    @Input()
    title: string;

    @Input() beforeRowClick: AdminList.CheckRowFun;

    @Input() canRemoveRow: AdminList.CheckRowFun;

    @Input() useDragDrop = false;

    @Input()
    rowClickMode: 'navigate' | 'none' = 'navigate';

    @Input() viewOptions: HlcNzTable.ViewOptions = {
        hidePager: false,
        hideSort: false,
    };

    @Input()
    removeItemDataAccess: AdminList.Data.RemoveItemDataAccess | undefined;

    @Output()
    actionClick = new EventEmitter<ActionClickEvent>();

    @Output()
    rowClick = new EventEmitter<RowClickEvent>();

    @Output()
    cellClick = new EventEmitter<CellClickEvent>();

    @Output()
    rowDrop = new EventEmitter<RowDropEvent>();

    @ContentChild(AdminListHeaderComponent, { static: false })
    header: AdminListHeaderComponent;

    @ViewChild(HlcNzTableComponent, { static: false })
    hlcList: HlcNzTableComponent;

    private readonly destroy$ = new Subject();
    hlcDefinition: HlcNzTable.TableDefinition;

    private readonly translate = this.transloco.translate.bind(this.transloco);

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
        private readonly filterService: HlcNzTableFilterService,
        private readonly transloco: TranslocoService,
        @Optional()
        @SkipSelf()
        @Inject(HLC_NZ_TABLE_CUSTOM_CELLS_PROVIDER)
        private readonly containerCustomCellsProvider?: HlcNzTableCustomCellsProvider
    ) {}

    ngOnInit() {
        this.hlcDefinition = this.definition;

        this.hlcDefinition = addDefinitionTranslations(
            this.translate,
            this.hlcDefinition
        );

        this.hlcDefinition = this.hlcDefinition.layout
            ? this.hlcDefinition
            : { ...this.hlcDefinition };

        if (this.definition.hasLinkButton) {
            const linkButtonTitle =
                typeof this.definition.hasLinkButton === 'string'
                    ? this.definition.hasLinkButton
                    : null;

            this.hlcDefinition = addDefinitionLinkButtonAction(
                this.translate,
                linkButtonTitle,
                this.hlcDefinition
            );
        }

        if (this.removeItemDataAccess) {
            this.hlcDefinition = addDefinitionDeleteButtonAction(
                this.translate,
                this.hlcDefinition,
                this.canRemoveRow
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
        if (this.beforeRowClick && !this.beforeRowClick($event.row)) {
            return;
        }
        if (this.rowClickMode === 'navigate') {
            this.router.navigate(['.', $event.row.id], {
                relativeTo: this.activatedRoute,
            });
        }
    }

    onCellClick($event: CellClickEvent) {
        this.cellClick.emit($event);
    }

    async onRowDrop($event: RowDropEvent) {}

    onActionClick(action: ActionClickEvent) {
        this.actionClick.emit(action);
        if (action.actionId === DELETE_ACTION_ID) {
            this.modalService.confirm({
                nzTitle: this.translate('deletItemTitle'),
                nzContent: this.translate('deletItemContent'),
                nzOkText: this.translate('yes'),
                nzCancelText: this.translate('no'),
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

    setFilter(filter: FilterValue) {
        this.filterService.setValue(filter);
    }

    get customCells() {
        return concat(
            this.customCellsContent ? this.customCellsContent.toArray() : [],
            (this.containerCustomCellsProvider &&
                this.containerCustomCellsProvider.customCells) ||
                []
        );
    }

    onDataProviderError(err: Error | null) {
        this.errorMessage = err && err.message;
    }
}
