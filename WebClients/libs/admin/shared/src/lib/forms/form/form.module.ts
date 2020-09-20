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
    NzSpinModule,
} from 'ng-zorro-antd';

import {
    adminFieldContainersComponents,
    adminFieldContainersMap,
    adminFieldContainersModules,
} from './field-containers-map';
import {
    adminFieldsComponents,
    adminFieldsMap,
    adminFieldsModules,
} from './fields-map';
import { AdminFormPageComponent } from './form-page/form-page.component';
import { AdminFormComponent } from './form/form.component';

@NgModule({
    declarations: [AdminFormComponent, AdminFormPageComponent],
    exports: [AdminFormComponent, AdminFormPageComponent],
    imports: [
        BrowserModule,
        HlcNzFormModule,
        NzAlertModule,
        NzButtonModule,
        NzDrawerModule,
        NzSpinModule,
        RouterModule,
        ...adminFieldsModules,
        ...adminFieldContainersModules,
    ],
    providers: [
        {
            provide: HLC_FIELDS_LAYOUT_MAP,
            multi: true,
            useValue: adminFieldsMap,
        },
        {
            provide: HLC_FORM_FIELD_WRAPPER_MAP,
            multi: true,
            useValue: adminFieldContainersMap,
        },
        {
            provide: HLC_NZ_VALIDATION_ERRORS_MAP_CONFIG,
            useValue: hlcNzDefaultErrorsMapConfig,
        },
        HlcNzInputErrorDisplayStrategy,
    ],
    entryComponents: [
        ...adminFieldsComponents,
        ...adminFieldContainersComponents,
    ],
})
export class AdminFormModule {}
