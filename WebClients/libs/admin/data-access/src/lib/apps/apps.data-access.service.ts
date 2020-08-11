import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { AppItem } from './models';
import { mapListRequestParams } from '../utils';

@Injectable()
export class AppsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<AppItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return of({
            data: [
                {
                    id: 1,
                    name: 'first',
                    clientId: 'xxx',
                    idTokenExpiresIn: 10,
                    refreshTokenExpiresIn: 2500,
                    dateCreated: new Date().toISOString(),
                },
            ],
            pager: { total: 1, size: 1, index: 1 },
        });
        //return this.http.get(BLOG_PATH, { params }).pipe(map(mapListResponse(mapItem, searchParams)));
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
