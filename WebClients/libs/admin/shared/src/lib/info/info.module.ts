import { NgModule } from '@angular/core';
import { InfoComponent } from './info.component';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { EffectsModule } from '@ngrx/effects';
import { InfoEffects } from './ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { infoReducer } from './ngrx/reducer';
import { CommonModule } from '@angular/common';

@NgModule({
    declarations: [InfoComponent],
    exports: [InfoComponent],
    imports: [
        CommonModule,
        NzToolTipModule,
        NzIconModule,
        EffectsModule.forFeature([InfoEffects]),
        StoreModule.forFeature('info', infoReducer),
    ],
    providers: [],
})
export class InfoModule {}
