import { AdminForm } from '@admin/shared';
import { Validators } from '@angular/forms';

export const definition: AdminForm.FormDefinition = {
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
                            kind: 'Display',
                            label: 'Signing Algorithm',
                        },
                        {
                            id: 'hS256SigningSecret',
                            kind: 'Display',
                            label: 'Signing Secret',
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
};
