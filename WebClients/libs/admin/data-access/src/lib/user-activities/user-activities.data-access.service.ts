import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { mapListRequestParams, mapListResponse } from '../utils';

const mapItem = (x) => ({
    ...x,
    id: x._id,
});

@Injectable()
export class UserActivitiesDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadUsersList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<any>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.email'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/user-activities`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    loadAdminsList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<any>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.email'] = searchParams.filter.text;
        }
        return this.http
            .get(`/tenant/domains/${domainId}/admin-activities`, {
                params,
            })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }
}
