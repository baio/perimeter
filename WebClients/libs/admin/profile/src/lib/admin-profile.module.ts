import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataAccessModule } from '@admin/data-access';
import { EffectsModule } from '@ngrx/effects';
import { ProfileEffects } from './effects';
import { StoreModule } from '@ngrx/store';
import { profileReducer } from './reducer';

@NgModule({
    imports: [
        CommonModule,
        DataAccessModule,
        EffectsModule.forFeature([ProfileEffects]),
        StoreModule.forFeature('profile', profileReducer),
    ],
})
export class AdminProfileModule {}
