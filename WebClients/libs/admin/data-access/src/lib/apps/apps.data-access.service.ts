import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { AppItem } from './models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const mapItem = (x) => x;

@Injectable()
export class AppsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<AppItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/applications`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(
        id: number
    ): Observable<HlcNzTable.Data.DataProviderResult<any>> {
        return of(null);
    }

    loadItem(id: number): Observable<AppItem> {
        return of({
            id: 1,
            name: 'first',
            clientId: 'xxx',
            idTokenExpiresIn: 10,
            refreshTokenExpiresIn: 2500,
            dateCreated: new Date().toISOString(),
        });
    }

    createItem(data: Partial<AppItem>): Observable<AppItem> {
        return of(data as any);
    }

    updateItem(id: number, data: Partial<AppItem>): Observable<AppItem> {
        return of(data as any);
    }
}
