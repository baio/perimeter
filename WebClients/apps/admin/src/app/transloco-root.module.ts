import { HttpClient } from '@angular/common/http';
import {
    TRANSLOCO_LOADER,
    Translation,
    TranslocoLoader,
    TRANSLOCO_CONFIG,
    translocoConfig,
    TranslocoModule,
    TranslocoService,
    getBrowserLang,
} from '@ngneat/transloco';
import { APP_INITIALIZER, Injectable, NgModule } from '@angular/core';
import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
    constructor(private http: HttpClient) {}

    getTranslation(lang: string) {
        return this.http.get<Translation>(`../assets/i18n/${lang}.json`);
    }
}

export function preloadUser(transloco: TranslocoService) {
    return function () {
        const appLang = localStorage.getItem('APP_LANG');
        let lang = appLang || getBrowserLang();
        console.log('lang', lang);
        if (lang !== 'ru') {
            lang = 'en';
        }
        localStorage.setItem('APP_LANG', lang);
        transloco.setDefaultLang(lang);
        transloco.setActiveLang(lang);
        return transloco.load(lang).toPromise();
    };
}

@NgModule({
    exports: [TranslocoModule],
    providers: [
        {
            provide: TRANSLOCO_CONFIG,
            useValue: translocoConfig({
                availableLangs: ['en', 'ru'],
                defaultLang: 'en',
                // Remove this option if your application
                // doesn't support changing language in runtime.
                reRenderOnLangChange: true,
                prodMode: environment.production,
            }),
        },
        { provide: TRANSLOCO_LOADER, useClass: TranslocoHttpLoader },
        {
            provide: APP_INITIALIZER,
            multi: true,
            useFactory: preloadUser,
            deps: [TranslocoService],
        },
    ],
})
export class TranslocoRootModule {}
