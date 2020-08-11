import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { DomainItem } from './models';
import { mapListRequestParams } from '../utils';

@Injectable()
export class DomainsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<DomainItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return of({
            data: [
                {
                    id: 1,
                    name: 'first',
                    dateCreated: new Date().toISOString(),
                    envs: [
                        {
                            id: 1,
                            name: 'dev',
                            isMain: true,
                        },
                    ],
                },
            ],
            pager: { total: 1, size: 1, index: 1 },
        });
        //return this.http.get(BLOG_PATH, { params }).pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(
        id: number
    ): Observable<HlcNzTable.Data.DataProviderResult<DomainItem>> {
        return of(null);
    }
}
