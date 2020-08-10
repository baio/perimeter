import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { DomainsDataAccessService } from './domains/domains.data-access.service';

@NgModule({
    imports: [HttpClientModule],
    declarations: [],
    providers: [DomainsDataAccessService],
})
export class DataAccessModule {}
