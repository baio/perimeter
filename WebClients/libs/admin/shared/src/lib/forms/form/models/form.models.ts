import { HlcNzForm } from '@nz-holistic/nz-forms';
import { Observable } from 'rxjs';
import { AdminFormFields } from './form-fields.models';

export namespace AdminForm {
    export type FieldsLayout = HlcNzForm.FieldsLayout<AdminFormFields.FormField, AdminFormFields.FieldWrapper>;
    export type FormDefinition = HlcNzForm.FormDefinition<AdminFormFields.FormField, AdminFormFields.FieldWrapper>;

    export namespace Data {
        export type StoreValueDataAccess = (item: any, currentValue?: any) => Observable<any>;
        export type LoadValueDataAccess = (id: number | string) => Observable<any>;
        export type RemoveItemDataAccess = (item: any) => Observable<any>;
    }
}
