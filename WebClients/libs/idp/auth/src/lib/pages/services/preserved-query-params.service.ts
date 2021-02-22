import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

/*
http://localhost:4201/auth/login?
client_id=__DEFAULT_CLIENT_ID__&
response_type=code&state=%7B%22nonce%22:%22-G9adZpSNN8hM52d6ir87utzPOwJGL8wMjfUN~5hvHSU~z_fdGVAtUAtJTw.O-l~%22,%22
redirectPath%22:%22%2F%22%7D&redirect_uri=http:%2F%2Flocalhost:4201%2Flogin-cb&scope=openid%20profile&
code_challenge=ssSZRsKogYInSjVGuBbpvkcvJfHF2EDnutfpn5qv7i4&
code_challenge_method=S256&
error=unauthorized_client&
error_description=Wrong%20email%20or%20password
*/
const preservedParams = [
    'client_id',
    'redirect_uri',
    'scope',
    'state',
    'response_type',
    'code_challenge',
    'code_challenge_method',
];

export type AuthParamName =
    | 'client_id'
    | 'redirect_uri'
    | 'scope'
    | 'state'
    | 'response_type'
    | 'code_challenge'
    | 'code_challenge_method';

@Injectable()
export class PreservedQueryParamsService {
    constructor(private readonly activatedRoute: ActivatedRoute) {}

    getAuthParam(key: AuthParamName) {
        return this.activatedRoute.snapshot.queryParamMap.get(key);
    }

    getAuthParams() {
        const queryParams = this.activatedRoute.snapshot.queryParamMap;
        const res = preservedParams.reduce((acc, k) => {
            const v = queryParams.get(k);
            if (v) {
                return { ...acc, [k]: v };
            } else {
                return acc;
            }
        }, {});
        return res;
    }

    /*
    getAuthParamsQueryString() {
        const authParams = this.getAuthParams();
        return Object.keys(authParams)
            .map((k) => `${k}=${encodeURIComponent(authParams[k])}`)
            .join('&');
    }
    */
}
