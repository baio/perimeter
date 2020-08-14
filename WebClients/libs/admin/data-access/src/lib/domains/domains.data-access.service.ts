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
    dateCreated: item.dateCreated,
    envs: item.domains.map((m) => ({
        id: m.id,
        name: m.envName,
        isMain: m.isMain,
    })),
});

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
        return this.http
            .get(`tenant/domain-pools`, { params })
            .pipe(map(mapListResponse(mapItem, searchParams)));
    }

    loadItem(id: number): Observable<DomainItem> {
        return this.http.get(`tenant/domain-pools/${id}`).pipe(map(mapItem));
    }

    createItem(data: { name: string }): Observable<any> {
        return this.http.post(`tenant/domain-pools`, data);
    }

    createEnvItem(
        domainId: number,
        data: { envName: string }
    ): Observable<any> {
        return this.http.post(`tenant/domain-pools/${domainId}/domains`, data);
    }

    updateItem(id: number, data: { name: string }): Observable<any> {
        return this.http.put(`tenant/domain-pools/${id}`, data);
    }

    removeItem(id: number): Observable<any> {
        return this.http.delete(`tenant/domain-pools/${id}`);
    }
}
