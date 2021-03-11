import { createFeatureSelector, createSelector } from '@ngrx/store';
import { InfoState } from './reducer';

export const selectInfoState = createFeatureSelector<InfoState>('info');

export const selectInfoLocale = (locale: string) =>
    createSelector(selectInfoState, (x) => x[locale]);

export const selectInfoLocaleKey = (locale: string, key: string) =>
    createSelector(selectInfoLocale(locale), (x) => x && x[key]);

export const selectInfoLocaleKeyText = (locale: string, key: string) =>
    createSelector(selectInfoLocaleKey(locale, key), (x) => x && x.text);
