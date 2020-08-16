import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { ApiItem } from './models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const mapItem = (x) => x;

const mapPayload = (x) => {
    const m = { ...x };
    delete m.permissions;
    return m;
};

@Injectable()
export class ApisDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<ApiItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/apis`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(domainId: number, id: number): Observable<any> {
        return this.http.delete(`/tenant/domains/${domainId}/apis/${id}`);
    }

    loadItem(domainId: number, id: number): Observable<ApiItem> {
        return this.http.get<ApiItem>(`/tenant/domains/${domainId}/apis/${id}`);
    }

    createItem(domainId: number, data: Partial<ApiItem>): Observable<ApiItem> {
        return this.http.post<ApiItem>(
            `/tenant/domains/${domainId}/apis`,
            mapPayload(data)
        );
    }

    updateItem(
        domainId: number,
        id: number,
        data: Partial<ApiItem>
    ): Observable<ApiItem> {
        return this.http.put<ApiItem>(
            `/tenant/domains/${domainId}/apis/${id}`,
            mapPayload(data)
        );
    }
}
