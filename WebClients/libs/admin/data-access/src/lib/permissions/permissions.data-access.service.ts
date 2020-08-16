import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { mapListRequestParams, mapListResponse } from '../utils';
import { Permission } from '../models';
import { map } from 'rxjs/operators';

const mapItem = (x) => x;

@Injectable()
export class PermissionsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        apiId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<Permission>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.text'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/apis/${apiId}/permissions`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(apiId: number, id: number): Observable<any> {
        return this.http.delete(`/tenant/apis/${apiId}/permissions/${id}`);
    }

    loadItem(apiId: number, id: number): Observable<Permission> {
        return this.http
            .get(`/tenant/apis/${apiId}/permissions/${id}`)
            .pipe(map(mapItem));
    }

    createItem(
        apiId: number,
        data: Partial<Permission>
    ): Observable<Permission> {
        return this.http.post<Permission>(
            `/tenant/apis/${apiId}/permissions`,
            data
        );
    }

    updateItem(
        apiId: number,
        id: number,
        data: Partial<Permission>
    ): Observable<Permission> {
        return this.http.put<Permission>(
            `/tenant/apis/${apiId}/permissions/${id}`,
            data
        );
    }
}
