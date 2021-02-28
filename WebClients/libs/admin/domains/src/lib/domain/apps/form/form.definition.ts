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
            label: 'Client Id',
            hidden: isNew$,
        },
        {
            id: 'name',
            kind: 'Text',
            label: 'Name',
            validators: [Validators.required],
        },
        {
            id: 'grantTypes',
            kind: 'Select',
            props: {
                label: 'Grant Types',
                mode: 'tags',
                items: grantTypes,
                value: ['AuthorizationCodePKCE', 'RefreshToken'],
            },
            validators: [Validators.required],
        },
        {
            id: 'idTokenExpiresIn',
            kind: 'Number',
            label: 'ID Token Expires In (minutes)',
            validators: [Validators.required],
            hidden: isNew$,
        },
        {
            id: 'refreshTokenExpiresIn',
            kind: 'Number',
            label: 'Refresh Token Expires In (minutes)',
            validators: [Validators.required],
            hidden: isNew$,
        },
        {
            id: 'allowedCallbackUrls',
            kind: 'TextArea',
            label: 'Allowed return URIs',
            validators: [Validators.required],
            hidden: isNew$,
        },
        {
            id: 'allowedLogoutCallbackUrls',
            kind: 'TextArea',
            label: 'Allowed logout return URIs',
            validators: [Validators.required],
            hidden: isNew$,
        },
        {
            id: 'ssoEnabled',
            kind: 'Toggle',
            label: 'Use SSO',
            hidden: isNew$,
        },
    ],
});
