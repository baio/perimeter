import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { ApiItem } from './models';
import { mapListRequestParams } from '../utils';

@Injectable()
export class ApisDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<ApiItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return of({
            data: [
                {
                    id: 1,
                    name: 'first',
                    identifier: 'xxx',
                    permissions: [],
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

    loadItem(id: number): Observable<ApiItem> {
        return of({
            id: 1,
            name: 'first',
            identifier: 'xxx',
            permissions: [],
            dateCreated: new Date().toISOString(),
        });
    }

    createItem(data: Partial<ApiItem>): Observable<ApiItem> {
        return of(data as any);
    }

    updateItem(id: number, data: Partial<ApiItem>): Observable<ApiItem> {
        return of(data as any);
    }
}
