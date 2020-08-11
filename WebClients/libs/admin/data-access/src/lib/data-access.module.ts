import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { DomainsDataAccessService } from './domains/domains.data-access.service';
import { AppsDataAccessService } from './apps/apps.data-access.service';
import { ApisDataAccessService } from './apis/apis.data-access.service';
import { PermissionsDataAccessService } from './permissions/permissions.data-access.service';
import { RolesDataAccessService } from './roles/roles.data-access.service';
import { UsersDataAccessService } from './users/users.data-access.service';
import { AdminsDataAccessService } from './admins/admins.data-access.service';

@NgModule({
    imports: [HttpClientModule],
    declarations: [],
    providers: [
        DomainsDataAccessService,
        ApisDataAccessService,
        AppsDataAccessService,
        DomainsDataAccessService,
        PermissionsDataAccessService,
        RolesDataAccessService,
        UsersDataAccessService,
        AdminsDataAccessService,
    ],
})
export class DataAccessModule {}
