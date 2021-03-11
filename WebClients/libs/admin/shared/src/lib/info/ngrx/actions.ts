import { createAction, props } from '@ngrx/store';
import { Info } from '../models/info.model';

export const loadInfo = createAction('[Info] Load Info');

export const loadInfoSuccess = createAction(
    '[Info] Load Info Success',
    props<{ items: Info[] }>()
);
