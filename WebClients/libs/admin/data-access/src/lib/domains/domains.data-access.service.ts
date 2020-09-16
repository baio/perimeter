import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { DomainItem } from './models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const mapItem = (item) => ({
    id: item.id,
    name: item.name,
    identifier: item.identifier,
    dateCreated: item.dateCreated,
    envs: item.domains.map((m) => ({
        id: m.id,
        name: m.envName,
        isMain: m.isMain,
        domainManagementClientId: m.domainManagementClientId,
    })),
});

@Injectable()
export class DomainsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        tenantId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<HlcNzTable.Data.DataProviderResult<DomainItem>> {
        const params = mapListRequestParams(searchParams);
        if (searchParams.filter && searchParams.filter.text) {
            params['filter.name'] = searchParams.filter.text;
        }
        return this.http
            .get(`tenants/${tenantId}/domain-pools`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    loadItem(tenantId: number, id: number): Observable<DomainItem> {
        return this.http
            .get(`tenants/${tenantId}/domain-pools/${id}`)
            .pipe(map(mapItem));
    }

    createItem(tenantId: number, data: { name: string }): Observable<any> {
        return this.http.post(`tenants/${tenantId}/domain-pools`, data);
    }

    createEnvItem(
        domainPoolId: number,
        data: { envName: string }
    ): Observable<any> {
        return this.http.post(
            `tenant/domain-pools/${domainPoolId}/domains`,
            data
        );
    }

    updateEnvItem(
        domainPoolId: number,
        domainId: number,
        data: { envName: string }
    ): Observable<any> {
        return this.http.put(
            `tenant/domain-pools/${domainPoolId}/domains/${domainId}`,
            data
        );
    }

    loadEnvItem(domainPoolId: number, domainId: number): Observable<any> {
        return this.http.get(
            `tenant/domain-pools/${domainPoolId}/domains/${domainId}`
        );
    }

    updateItem(
        tenantId: number,
        id: number,
        data: { name: string }
    ): Observable<any> {
        return this.http.put(`tenants/${tenantId}/domain-pools/${id}`, data);
    }

    removeItem(tenantId: number, id: number): Observable<any> {
        return this.http.delete(`tenants/${tenantId}/domain-pools/${id}`);
    }
}
