import { AdminForm } from '@admin/shared';
import { Validators } from '@angular/forms';
import { Observable } from 'rxjs';

const grantTypes = [
    {
        key: 'AuthorizationCode',
        label: 'Authorization Code',
    },
    {
        key: 'AuthorizationCodePKCE',
        label: 'Authorization Code PKCE',
    },
    {
        key: 'Password',
        label: 'Password',
    },
    {
        key: 'RefreshToken',
        label: 'Refresh Token',
    },
    {
        key: 'ClientCredentials',
        label: 'Client Credentials',
    },
];

export const getDefinition = (
    isNew$: Observable<boolean>
): AdminForm.FormDefinition => ({
    kind: 'fields',
    fields: [
        {
            id: 'clientId',
            kind: 'Display',
            label: 'clientId',
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:clientId',
                },
            },
        },
        {
            id: 'name',
            kind: 'Text',
            label: 'name',
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:name',
                },
            },
        },
        {
            id: 'grantTypes',
            kind: 'Select',
            props: {
                label: 'grantTypes',
                mode: 'tags',
                items: grantTypes,
                value: ['AuthorizationCodePKCE', 'RefreshToken'],
            },
            validators: [Validators.required],
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:grantTypes',
                },
            },
        },
        {
            id: 'idTokenExpiresIn',
            kind: 'Number',
            label: 'idTokenExpiresIn',
            validators: [Validators.required],
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:idTokenExpiresIn',
                },
            },
        },
        {
            id: 'refreshTokenExpiresIn',
            kind: 'Number',
            label: 'refreshTokenExpiresIn',
            validators: [Validators.required],
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:refreshTokenExpiresIn',
                },
            },
        },
        {
            id: 'allowedCallbackUrls',
            kind: 'TextArea',
            label: 'returnUris',
            validators: [Validators.required],
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:allowedCallbackUrls',
                },
            },
        },
        {
            id: 'allowedLogoutCallbackUrls',
            kind: 'TextArea',
            label: 'logoutReturnUris',
            validators: [Validators.required],
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:allowedLogoutCallbackUrls',
                },
            },
        },
        {
            id: 'ssoEnabled',
            kind: 'Toggle',
            label: 'useSSO',
            hidden: isNew$,
            wrapper: {
                kind: 'info',
                props: {
                    infoKey: 'domain:ssoEnabled',
                },
            },
        },
    ],
});
