import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserRole } from '../models';
import { mapListRequestParams, mapListResponse } from '../utils';

const mapItem = (x) => ({
    ...x,
    id: x.userEmail,
    rolesIds: x.roles.map((m) => m.id),
    email: x.userEmail,
});

@Injectable()
export class UsersDataAccessService {
    constructor(private readonly http: HttpClient) {}

    getAllRoles(domainId: number): Observable<{ id: number; name: string }[]> {
        return this.http.get<any[]>(
            `/tenant/domains/${domainId}/roles/users`
        );
    }

    loadItem(domainId: number, userEmail: string): Observable<UserRole> {
        return this.http
            .get(`/tenant/domains/${domainId}/users/${userEmail}/roles`)
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
            .get(`/tenant/domains/${domainId}/users/roles`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    removeItem(domainId: number, userEmail: string): Observable<any> {
        return this.http.delete(
            `/tenant/domains/${domainId}/users/${userEmail}/roles`
        );
    }

    updateItem(
        domainId: number,
        data: Partial<UserRole>
    ): Observable<UserRole> {
        return this.http.post<UserRole>(
            `/tenant/domains/${domainId}/users/roles`,
            data
        );
    }
}
