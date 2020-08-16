import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { RoleItem } from './models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const mapItem = (x) => x;

@Injectable()
export class RolesDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<RoleItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.text'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/roles`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(domainId: number, id: number): Observable<any> {
        return this.http.delete(`/tenant/domains/${domainId}/roles/${id}`);
    }

    loadItem(domainId: number, id: number): Observable<RoleItem> {
        return this.http
            .get(`/tenant/domains/${domainId}/roles/${id}`)
            .pipe(map(mapItem));
    }

    createItem(
        domainId: number,
        data: Partial<RoleItem>
    ): Observable<RoleItem> {
        return this.http.post<RoleItem>(
            `/tenant/domains/${domainId}/roles`,
            data
        );
    }

    updateItem(
        domainId: number,
        id: number,
        data: Partial<RoleItem>
    ): Observable<RoleItem> {
        return this.http.put<RoleItem>(
            `/tenant/domains/${domainId}/roles/${id}`,
            data
        );
    }
}
