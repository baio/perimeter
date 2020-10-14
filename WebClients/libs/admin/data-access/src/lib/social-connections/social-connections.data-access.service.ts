import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HlcNzTable } from '@nz-holistic/nz-list';
import { Observable, of } from 'rxjs';
import { SocialConnection } from './models';
import { mapListRequestParams, mapListResponse } from '../utils';
import { map } from 'rxjs/operators';

const socialConnections: SocialConnection[] = [
    {
        id: 'twitter',
        name: 'twitter',
        isEnabled: false,
        clientId: null,
        clientSecret: null,
        attributes: null,
        permissions: null,
    },
];

@Injectable()
export class SocialConnectionsDataAccessService {
    constructor(private readonly http: HttpClient) {}

    loadList(
        domainId: number,
        searchParams: HlcNzTable.Data.DataProviderState
    ): Observable<SocialConnection[]> {
        return this.http
            .get<SocialConnection[]>(`/tenant/domains/${domainId}/social`)
            .pipe(
                map((enabled) => [
                    ...enabled.map((x) => ({
                        ...x,
                        id: x.name,
                        isEnabled: true,
                    })),
                    ...socialConnections.filter(
                        (f) => !enabled.some((x) => x.name === f.name)
                    ),
                ])
            );
    }

    loadItem(domainId: number, id: string): Observable<SocialConnection> {
        return this.loadList(domainId, null).pipe(
            map((result) => result.find((f) => f.id === id))
        );
    }

    removeItem(domainId: number, id: string): Observable<any> {
        return this.http.delete(`/tenant/domains/${domainId}/social/${id}`);
    }

    createItem(domainId: number, data: SocialConnection): Observable<any> {
        return this.http.post(
            `/tenant/domains/${domainId}/social/${data.name}`,
            data
        );
    }

    updateItem(domainId: number, data: SocialConnection): Observable<any> {
        return this.http.put(
            `/tenant/domains/${domainId}/social/${data.name}`,
            data
        );
    }
}
