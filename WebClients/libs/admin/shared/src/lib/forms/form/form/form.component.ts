import {
    AfterViewInit,
    Component,
    EventEmitter,
    Input,
    OnInit,
    Optional,
    Output,
    ViewChild,
} from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoService } from '@ngneat/transloco';
import { HlcNzFormComponent } from '@nz-holistic/nz-forms';
import { mapServerError } from '@perimeter/common';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { combineLatest, merge, Observable, of, Subject } from 'rxjs';
import {
    catchError,
    delay,
    distinctUntilChanged,
    flatMap,
    map,
    mapTo,
    shareReplay,
    startWith,
    switchMap,
    take,
    tap,
    withLatestFrom,
} from 'rxjs/operators';
import { AdminListService } from '../../../common';
import { AdminForm } from '../models';
import { addDefinitionTranslations, delayWhile } from './utils';

export type FormStatus = 'none' | 'loading' | 'submitting' | 'invalid';

export interface FormView {
    isNew: boolean;
    status: FormStatus;
    value: any;
    submitDisabled: boolean;
    error?: string;
}

export type AdminFormMode = 'none' | 'routed';

export interface FormCreatedEvent {
    form: FormGroup;
    loadedVale: any;
    itemId: any;
}

@Component({
    selector: 'admin-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.scss'],
})
export class AdminFormComponent implements OnInit, AfterViewInit {
    @Input()
    closeOnStoreSuccess = true;

    @Input()
    definition: AdminForm.FormDefinition;

    @Input()
    loadValueDataAccess: AdminForm.Data.LoadValueDataAccess;

    @Input()
    mode: AdminFormMode = 'routed';

    @Input()
    storeValueDataAccess: AdminForm.Data.StoreValueDataAccess;

    @Input()
    title: string;

    @Input()
    value: any;

    @Input()
    removeItemDataAccess: AdminForm.Data.RemoveItemDataAccess | undefined;

    @Output()
    formCreated = new EventEmitter<FormCreatedEvent>();

    // tslint:disable-next-line: no-output-native
    @Output() close = new EventEmitter();

    @ViewChild(HlcNzFormComponent, { static: false })
    hlcForm: HlcNzFormComponent;

    disabled$: Observable<boolean>;
    private readonly submit$ = new Subject<any>();
    private readonly remove$ = new Subject<any>();
    view$: Observable<FormView>;
    hlcDefinition: AdminForm.FormDefinition;

    private readonly translate = this.transloco.translate.bind(this.transloco);

    get itemId$(): Observable<number | string> {
        return this.mode === 'routed'
            ? this.activatedRoute.params.pipe(
                  map(({ id }) => (id === 'new' ? null : +id || (id as string)))
              )
            : of(null);
    }

    constructor(
        private readonly router: Router,
        private readonly activatedRoute: ActivatedRoute,
        private readonly notificationService: NzNotificationService,
        private readonly modalService: NzModalService,
        private readonly transloco: TranslocoService,
        @Optional() private readonly listService?: AdminListService
    ) {}

    ngOnInit() {
        this.hlcDefinition = addDefinitionTranslations(
            this.translate,
            this.definition
        );

        const value$ =
            this.value || !this.loadValueDataAccess
                ? of(this.value)
                : this.itemId$.pipe(
                      switchMap((id) =>
                          id !== null ? this.loadValueDataAccess(id) : of(null)
                      ),
                      shareReplay(1)
                  );

        const valueLoad$ = value$.pipe(
            map((value) => ({
                status: 'none' as FormStatus,
                value,
                error: null,
            })),
            startWith({
                status: 'loading' as FormStatus,
                value: null,
                error: null,
            })
        );

        const submit$ = this.submit$.pipe(
            withLatestFrom(value$),
            map(([formValue, value]) => ({ ...value, ...formValue }))
        );

        const valueSubmit$ = merge(
            submit$.pipe(
                map((value) => ({
                    status: 'submitting' as FormStatus,
                    value,
                    error: null,
                }))
            ),
            submit$.pipe(
                withLatestFrom(value$),
                switchMap(([value, currentValue]) =>
                    this.storeValueDataAccess(value, currentValue).pipe(
                        tap((res) => {
                            if (this.listService) {
                                value.id
                                    ? this.listService.onRowUpdated(res)
                                    : this.listService.onRowAdded(res);
                            }
                            this.form.markAsPristine();
                        }),
                        map((res) => ({
                            status: 'none' as FormStatus,
                            value: res,
                            error: null,
                        })),
                        tap(() => {
                            if (this.closeOnStoreSuccess) {
                                const successMessage = value.id
                                    ? this.translate('messageUpdateSuccess')
                                    : this.translate('messageCreateSuccess');
                                this.notificationService.success(
                                    successMessage,
                                    ''
                                );
                                this.onClose();
                            }
                        }),
                        catchError((error) => {
                            const msg = mapServerError(this.form, error);
                            return of({
                                status: 'none' as FormStatus,
                                value,
                                error: msg,
                            });
                        })
                    )
                )
            )
        );

        const itemRemove$ = merge(
            this.remove$.pipe(
                map((value) => ({
                    status: 'submitting' as FormStatus,
                    value,
                    error: null,
                }))
            ),
            this.remove$.pipe(
                switchMap((value) =>
                    this.removeItemDataAccess(value).pipe(
                        tap(() => {
                            const successMessage = this.translate(
                                'messageRemoveSuccess'
                            );
                            this.notificationService.success(
                                successMessage,
                                ''
                            );
                            this.onClose();
                            if (this.listService) {
                                this.listService.onRowRemoved(value);
                            }
                        }),
                        catchError((error) => {
                            return of({
                                status: 'none' as FormStatus,
                                value,
                                error,
                            });
                        })
                    )
                )
            )
        );

        const valueWithStatus$ = merge(
            valueLoad$,
            valueSubmit$,
            itemRemove$
        ).pipe(shareReplay(1));

        const formCreated$ = delayWhile(valueWithStatus$, () => !!this.form);

        // Give chance to render form before subscribe form value
        const formValueChanged$ = formCreated$.pipe(
            // give chance to update form field status to dirty
            delay(0),
            flatMap(() => this.form.valueChanges.pipe(mapTo(this.form)))
        );

        const statusChanged$ = valueWithStatus$.pipe(
            map(({ status }) => status),
            distinctUntilChanged()
        );

        const submitDisabled$ = combineLatest([
            formValueChanged$,
            statusChanged$,
        ]).pipe(
            delay(0),
            map(([form, status]) => {
                return (
                    !form || form.pristine || form.invalid || status !== 'none'
                );
            }),
            startWith(true),
            distinctUntilChanged()
        );

        this.view$ = combineLatest([
            valueWithStatus$,
            this.itemId$,
            submitDisabled$,
        ]).pipe(
            map(([{ value, status, error }, itemId, submitDisabled]) => ({
                value,
                status,
                isNew: itemId === null,
                error,
                submitDisabled,
            }))
        );

        formCreated$
            .pipe(take(1), withLatestFrom(this.view$, this.itemId$))
            .subscribe(([_, val, itemId]) => {
                this.formCreated.emit({
                    form: this.form,
                    loadedVale: val.value,
                    itemId,
                });
            });
    }

    ngAfterViewInit() {}

    get form() {
        return this.hlcForm && this.hlcForm.form;
    }

    isFormInvalid(form: FormGroup) {
        return this.form.invalid;
    }

    onClose() {
        if (this.mode === 'routed') {
            this.router.navigate(['..'], { relativeTo: this.activatedRoute });
        }
        this.close.emit();
    }

    onSubmit(value: any) {
        this.submit$.next(value);
    }

    onRemove(value: any) {
        this.modalService.confirm({
            nzTitle: this.translate('deletItemTitle'),
            nzContent: this.translate('deletItemContent'),
            nzOkText: this.translate('yes'),
            nzCancelText: this.translate('no'),
            nzOnOk: () => {
                this.remove$.next(value);
                return true;
            },
        });
    }
}
