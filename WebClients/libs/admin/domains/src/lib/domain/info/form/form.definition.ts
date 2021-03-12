import { AdminForm } from '@admin/shared';
import { FormGroup, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';

export const getDefinition = (form: FormGroup): AdminForm.FormDefinition => ({
    kind: 'tabs',
    $content: [
        {
            kind: 'tab',
            title: 'main',
            $content: [
                {
                    kind: 'fields',
                    fields: [
                        {
                            id: 'envName',
                            kind: 'Text',
                            label: 'name',
                            validators: [Validators.required],
                            wrapper: {
                                kind: 'info',
                                props: { infoKey: 'domain:name' },
                            },
                        },
                        {
                            id: 'issuer',
                            kind: 'Display',
                            label: 'issuer',
                            wrapper: {
                                kind: 'info',
                                props: { infoKey: 'domain:issuer' },
                            },
                        },
                    ],
                },
            ],
        },
        {
            kind: 'tab',
            title: 'accessToken',
            $content: [
                {
                    kind: 'fields',
                    fields: [
                        {
                            id: 'signingAlgorithm',
                            kind: 'Select',
                            label: 'signingAlgorithm',
                            props: {
                                mode: 'default',
                                items: [
                                    {
                                        key: 'HS256',
                                        label: 'HS256',
                                    },
                                    {
                                        key: 'RS256',
                                        label: 'RS256',
                                    },
                                ],
                            },
                            wrapper: {
                                kind: 'info',
                                props: { infoKey: 'domain:signing-algorithm' },
                            },
                        },
                        {
                            id: 'rS256PublicKey',
                            kind: 'Display',
                            label: 'rS256PublicKey',
                            hidden: form.valueChanges.pipe(
                                map(
                                    ({ signingAlgorithm }) =>
                                        signingAlgorithm !== 'RS256'
                                )
                            ),
                            wrapper: {
                                kind: 'info',
                                props: { infoKey: 'domain:rs-256-key' },
                            },
                        },
                        {
                            id: 'hS256SigningSecret',
                            kind: 'Display',
                            label: 'hS256SigningSecret',
                            hidden: form.valueChanges.pipe(
                                map(
                                    ({ signingAlgorithm }) =>
                                        signingAlgorithm !== 'HS256'
                                )
                            ),
                            wrapper: {
                                kind: 'info',
                                props: { infoKey: 'domain:hs-256-key' },
                            },
                        },
                        {
                            id: 'accessTokenExpiresIn',
                            kind: 'Number',
                            label: 'accessTokenExpiresIn',
                            validators: [Validators.required],
                            wrapper: {
                                kind: 'info',
                                props: {
                                    infoKey: 'domain:access-token-expires-in',
                                },
                            },
                        },
                    ],
                },
            ],
        },
    ],
});
