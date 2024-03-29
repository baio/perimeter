import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AdminsDataAccessService } from './admins/admins.data-access.service';
import { ApisDataAccessService } from './apis/apis.data-access.service';
import { AppsDataAccessService } from './apps/apps.data-access.service';
import { DomainsDataAccessService } from './domains/domains.data-access.service';
import { PermissionsDataAccessService } from './permissions/permissions.data-access.service';
import { ProfileDataAccessService } from './profile/profile.data-access.service';
import { RolesDataAccessService } from './roles/roles.data-access.service';
import { SocialConnectionsDataAccessService } from './social-connections/social-connections.data-access.service';
import { TenantAdminsDataAccessService } from './tenant-admins/tenant-admins.data-access.service';
import { TenantsDataAccessService } from './tenants/tenants.data-access.service';
import { UserActivitiesDataAccessService } from './user-activities/user-activities.data-access.service';
import { UsersDataAccessService } from './users/users.data-access.service';

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
        TenantAdminsDataAccessService,
        ProfileDataAccessService,
        TenantsDataAccessService,
        UserActivitiesDataAccessService,
        SocialConnectionsDataAccessService,
    ],
})
export class DataAccessModule {}
