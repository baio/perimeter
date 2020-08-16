import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HlcDefaultBindValue } from '@nz-holistic/forms';

export interface IPermission {
    id: number;
    name: string;
}

@Component({
    selector: 'admin-permissions-column',
    templateUrl: './permissions-column.component.html',
    styleUrls: ['./permissions-column.component.scss'],
})
export class AdminPermissionsColumnComponent {
    @HlcDefaultBindValue
    @Input()
    permissions: IPermission[];

    @Output() clicked = new EventEmitter<MouseEvent>();

    constructor() {}

    onClick(event: MouseEvent) {
        this.clicked.emit(event);
    }
}
