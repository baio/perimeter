import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { mapListRequestParams } from '../utils';
import { Permission } from '../models';

@Injectable()
export class PermissionsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<Permission>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return of({
            data: [
                {
                    id: 1,
                    name: 'first',
                    description: 'xxx',
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

    loadItem(id: number): Observable<Permission> {
        return of({
            id: 1,
            name: 'first',
            description: 'xxx',
        });
    }

    createItem(data: Partial<Permission>): Observable<Permission> {
        return of(data as any);
    }

    updateItem(id: number, data: Partial<Permission>): Observable<Permission> {
        return of(data as any);
    }
}
