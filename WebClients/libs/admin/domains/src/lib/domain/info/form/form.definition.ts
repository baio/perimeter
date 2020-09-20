import { AdminForm } from '@admin/shared';
import { FormGroup, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';

export const getDefinition = (form: FormGroup): AdminForm.FormDefinition => ({
    kind: 'tabs',
    $content: [
        {
            kind: 'tab',
            title: 'Main',
            $content: [
                {
                    kind: 'fields',
                    fields: [
                        {
                            id: 'envName',
                            kind: 'Text',
                            label: 'Name',
                            validators: [Validators.required],
                        },
                        {
                            id: 'issuer',
                            kind: 'Display',
                            label: 'Issuer',
                        },
                    ],
                },
            ],
        },
        {
            kind: 'tab',
            title: 'Access Token',
            $content: [
                {
                    kind: 'fields',
                    fields: [
                        {
                            id: 'signingAlgorithm',
                            kind: 'Select',
                            label: 'Signing Algorithm',
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
                        },
                        {
                            id: 'rS256PublicKey',
                            kind: 'Display',
                            label: 'Public Key',
                            hidden: form.valueChanges.pipe(
                                map(
                                    ({ signingAlgorithm }) =>
                                        signingAlgorithm !== 'RS256'
                                )
                            ),
                        },
                        {
                            id: 'hS256SigningSecret',
                            kind: 'Display',
                            label: 'Signing Secret',
                            hidden: form.valueChanges.pipe(
                                map(
                                    ({ signingAlgorithm }) =>
                                        signingAlgorithm !== 'HS256'
                                )
                            ),
                        },
                        {
                            id: 'accessTokenExpiresIn',
                            kind: 'Number',
                            label: 'Access Token Expires in Minutes',
                            validators: [Validators.required],
                        },
                    ],
                },
            ],
        },
    ],
});
