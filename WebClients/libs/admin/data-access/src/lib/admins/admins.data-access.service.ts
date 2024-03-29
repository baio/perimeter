import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { User, UserRole } from '../models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const mapItem = (x) => ({
    ...x,
    id: x.userEmail,
    roleId: x.roles.map((m) => m.id)[0],
    email: x.userEmail,
});

const mapPayload = (x) => ({
    ...x,
    rolesIds: x.roleId ? [x.roleId] : null,
});

@Injectable()
export class AdminsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    getAllRoles(): Observable<{ id: number; name: string }[]> {
        return this.http.get<any[]>(`/tenant/roles/admins`);
    }

    loadItem(domainId: number, userEmail: string): Observable<UserRole> {
        return this.http
            .get(`/tenant/domains/${domainId}/users/${userEmail}`)
            .pipe(map(mapItem));
    }

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<UserRole>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.email'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/admins`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(domainId: number, userEmail: string): Observable<any> {
        return this.http.delete(
            `/tenant/domains/${domainId}/users/${userEmail}`
        );
    }

    updateItem(
        domainId: number,
        data: Partial<UserRole>
    ): Observable<UserRole> {
        return this.http.post<UserRole>(
            `/tenant/domains/${domainId}/users`,
            mapPayload(data)
        );
    }
}
