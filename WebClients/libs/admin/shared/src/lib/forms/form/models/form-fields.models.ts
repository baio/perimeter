import { HlcFormFields } from '@nz-holistic/forms';
import {
    HlcNzFileUploadDataAccess,
    HlcNzFileUploadMode,
} from '@nz-holistic/nz-controls';
import { HlcNzFormFields } from '@nz-holistic/nz-forms';
import { NzTreeNodeOptions } from 'ng-zorro-antd/tree';
import { AdminImageEditorConfig } from '../../fields/image-upload/image-editor/image-editor.component';

export namespace AdminFormFields {
    export interface LengthCounterFieldWrapper {
        kind: 'lengthCounter';
        props: { maxLength: number; warnLength?: number };
    }
    export interface InfoFieldWrapper {
        kind: 'info';
        props: { infoKey: string };
    }

    export type FieldWrapper =
        | 'default'
        | LengthCounterFieldWrapper
        | InfoFieldWrapper;

    export type BaseField<
        TKind extends string,
        TVal = any,
        TExtProps = {
            infoKey?: string;
        }
    > = HlcNzFormFields.BaseField<TKind, TVal, TExtProps, FieldWrapper>;

    export interface StateSelector
        extends BaseField<'AdminStateSelector', string> {}
    export interface DateTimeDisplay
        extends BaseField<'AdminDateTimeDisplay', string> {}
    export interface RichText extends BaseField<'AdminRichText', string> {}
    export interface CategoriesSelector
        extends BaseField<
            'AdminCategoriesSelector',
            number[],
            { nodes: HlcFormFields.FormFieldProp<NzTreeNodeOptions[]> }
        > {}
    export interface ImageUpload
        extends BaseField<
            'AdminImageUpload',
            string,
            {
                editorConfig?: AdminImageEditorConfig;
                accept?: string;
                readonly?: HlcFormFields.FormFieldProp<boolean>;
                mode?: HlcNzFileUploadMode;
                dataAccess?: HlcNzFileUploadDataAccess;
                allowOrderItems?: boolean;
            }
        > {}

    // TODO : This one from level above
    export interface ItemCategoriesSelector
        extends BaseField<
            'AdminItemCategoriesSelector',
            number[],
            { category: string }
        > {}

    export type FormField =
        | StateSelector
        | DateTimeDisplay
        | RichText
        | CategoriesSelector
        | ItemCategoriesSelector
        | ImageUpload;
}
