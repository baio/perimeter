import { Component, Input, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import * as contentfull from 'contentful';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { selectInfoLocaleKeyText } from './ngrx/selectors';

@Component({
    selector: 'admin-info',
    template:
        '<i *ngIf="text$ | async as text" nz-tooltip [nzTooltipTitle]="text" nz-icon nzType="info-circle" nzTheme="outline"></i>',
})
export class InfoComponent implements OnInit {
    readonly text$: Observable<string>;
    private readonly key$ = new BehaviorSubject<string>(null);

    @Input() set key(key: string) {
        this.key$.next(key);
    }

    get key() {
        return this.key$.getValue();
    }

    constructor(store: Store) {        
        this.text$ = this.key$.pipe(
            filter((f) => !!f),
            switchMap((key) => {
                console.log('111', key);
                return store.select(selectInfoLocaleKeyText('ru-RU', key))
            })
        );
    }

    async ngOnInit() {}
}
