import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { DomainsDataAccessService } from './domains/domains.data-access.service';
import { AppsDataAccessService } from './apps/apps.data-access.service';
import { ApisDataAccessService } from './apis/apis.data-access.service';

@NgModule({
    imports: [HttpClientModule],
    declarations: [],
    providers: [
        DomainsDataAccessService,
        ApisDataAccessService,
        AppsDataAccessService,
    ],
})
export class DataAccessModule {}
