import { AdminPermissionsColumnComponent } from '../columns/permissions-column/permissions-column.component';
import { AdminPermissionsColumnModule } from '../columns/permissions-column/permissions-column.module';

export const tableCellMap = {
    Permissions: AdminPermissionsColumnComponent,
};

export const tableCellMapComponents = [AdminPermissionsColumnComponent];
export const tableCellMapModules = [AdminPermissionsColumnModule];
