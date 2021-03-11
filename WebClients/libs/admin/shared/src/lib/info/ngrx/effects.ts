import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import * as contentfull from 'contentful';
import { flatten } from 'lodash/fp';
import { switchMap } from 'rxjs/operators';
import { loadInfo, loadInfoSuccess } from './actions';

@Injectable()
export class InfoEffects {
    private readonly authService: contentfull.ContentfulClientApi;
    constructor(private readonly actions$: Actions) {
        this.authService = contentfull.createClient({
            space: '9znequornpbx',
            accessToken: '7o_vlWJlOwmm4Syxx7I657FtF2VYtamsoEVw6yFCy8U',
        });
    }

    load$ = createEffect(() =>
        this.actions$.pipe(
            ofType(loadInfo),
            switchMap(async () => {
                const result = await this.authService.getEntries({
                    locale: '*',
                });

                const items = flatten(
                    result.items.map((m) => {
                        const key =
                            m.fields['page']['en-US'] +
                                '-' +
                                ((m.fields['field'] &&
                                    m.fields['field']['en-US']) || 'page');
                        const url = m.fields['url'];
                        return [
                            {
                                key,
                                url,
                                locale: 'en-US',
                                text:
                                    m.fields['text'] &&
                                    m.fields['text']['en-US'].content
                                        .map((m) => m.content[0].value)
                                        .filter((f) => !!f)
                                        .join('\n'),
                            },
                            {
                                key,
                                url,
                                locale: 'ru-RU',
                                text:
                                    m.fields['text'] &&
                                    m.fields['text']['ru-RU']?.content
                                        .map((m) => m.content[0].value)
                                        .filter((f) => !!f)
                                        .join('\n'),
                            },
                        ];
                    })
                ).filter((f) => !!f.text);
                return loadInfoSuccess({ items });
            })
        )
    );
}
