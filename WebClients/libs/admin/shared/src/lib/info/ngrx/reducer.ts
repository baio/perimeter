import { createReducer, on } from '@ngrx/store';
import { assocPath } from 'lodash/fp';
import { Info } from '../models/info.model';
import { loadInfoSuccess } from './actions';

export interface InfoState {
    'en-US': {
        [key: string]: Info;
    };
    'ru-RU': {
        [key: string]: Info;
    };
}

export const initialState: InfoState = {
    'en-US': {},
    'ru-RU': {},
};

export const infoReducer = createReducer(
    initialState,
    on(loadInfoSuccess, (state, { items }) => {
        return items.reduce(
            (acc, v) => assocPath([v.locale, v.key], v, acc),
            initialState
        );
    })
);
