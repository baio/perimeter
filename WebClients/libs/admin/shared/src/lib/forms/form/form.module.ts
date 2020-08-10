import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import {
    HLC_FIELDS_LAYOUT_MAP,
    HLC_FORM_FIELD_WRAPPER_MAP,
} from '@nz-holistic/forms';
import {
    hlcNzDefaultErrorsMapConfig,
    HlcNzFormModule,
    HlcNzInputErrorDisplayStrategy,
    HLC_NZ_VALIDATION_ERRORS_MAP_CONFIG,
} from '@nz-holistic/nz-forms';
import {
    NzAlertModule,
    NzButtonModule,
    NzDrawerModule,
    NzSpinModule
} from 'ng-zorro-antd';

import {
    bazaFieldContainersComponents,
    bazaFieldContainersMap,
    bazaFieldContainersModules,
} from './field-containers-map';
import {
    bazaFieldsComponents,
    bazaFieldsMap,
    bazaFieldsModules,
} from './fields-map';
import { AdminFormComponent } from './form/form.component';

@NgModule({
    declarations: [AdminFormComponent],
    exports: [AdminFormComponent],
    imports: [
        BrowserModule,
        HlcNzFormModule,
        NzAlertModule,
        NzButtonModule,
        NzDrawerModule,
        NzSpinModule,
        RouterModule,
        ...bazaFieldsModules,
        ...bazaFieldContainersModules,
    ],
    providers: [
        {
            provide: HLC_FIELDS_LAYOUT_MAP,
            multi: true,
            useValue: bazaFieldsMap,
        },
        {
            provide: HLC_FORM_FIELD_WRAPPER_MAP,
            multi: true,
            useValue: bazaFieldContainersMap,
        },
        {
            provide: HLC_NZ_VALIDATION_ERRORS_MAP_CONFIG,
            useValue: hlcNzDefaultErrorsMapConfig,
        },
        HlcNzInputErrorDisplayStrategy,
    ],
    entryComponents: [
        ...bazaFieldsComponents,
        ...bazaFieldContainersComponents,
    ],
})
export class AdminFormModule {}
