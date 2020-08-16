import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { mapListRequestParams, mapListResponse } from '../utils';
import { User, UserRole } from '../models';
import { map } from 'rxjs/operators';

const mapItem = (x) => ({
    ...x,
    email: x.userEmail,
});

@Injectable()
export class UsersDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<UserRole>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/users/roles`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(id: number): Observable<any> {
        return this.http.delete(`/tenant/users/roles/${id}`);
    }

    loadItem(id: number): Observable<UserRole> {
        return this.http.get<UserRole>(`/tenant/users/roles/${id}`);
    }

    createItem(data: Partial<UserRole>): Observable<UserRole> {
        return this.http.post<UserRole>(`/tenant/users/roles`, data);
    }

    updateItem(id: number, data: Partial<UserRole>): Observable<UserRole> {
        return this.http.put<UserRole>(`/tenant/domains/users/${id}`, data);
    }
}
