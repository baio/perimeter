import { Component } from '@angular/core';
import { authenticate } from '@admin/profile';
import { Store } from '@ngrx/store';

@Component({
    selector: 'admin-blank-page',
    templateUrl: './blank-page.component.html',
    styleUrls: ['./blank-page.component.scss'],
})
export class BlankPageComponent {
    constructor(store: Store) {
        store.dispatch(authenticate());
    }
}
