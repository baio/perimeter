import { Component, Input } from '@angular/core';
import { HlcDefaultBindValue } from '@nz-holistic/forms';

export type BazaSate = 'PUBLISHED' | 'NOT_PUBLISHED' | 'ARCHIVED';

@Component({
    selector: 'admin-permissions-column',
    templateUrl: './permissions-column.component.html',
    styleUrls: ['./permissions-column.component.scss'],
})
export class AdminPermissionsColumnComponent {
    @HlcDefaultBindValue
    @Input()
    permissions: string[];

    constructor() {}
}
