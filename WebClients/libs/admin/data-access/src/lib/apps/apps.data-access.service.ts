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

    removeItem(domainId: number, id: number): Observable<any> {
        return this.http.delete(
            `/tenant/domains/${domainId}/applications/${id}`
        );
    }

    loadItem(domainId: number, id: number): Observable<AppItem> {
        return this.http.get<AppItem>(
            `/tenant/domains/${domainId}/applications/${id}`
        );
    }

    createItem(domainId: number, data: Partial<AppItem>): Observable<AppItem> {
        return this.http.post<AppItem>(
            `/tenant/domains/${domainId}/applications`,
            data
        );
    }

    updateItem(
        domainId: number,
        id: number,
        data: Partial<AppItem>
    ): Observable<AppItem> {
        return this.http.put<AppItem>(
            `/tenant/domains/${domainId}/applications/${id}`,
            data
        );
    }
}
