import {
    AdminDateTimeDisplayComponent,
    AdminDateTimeDisplayModule,
    AdminRichTextComponent,
    AdminRichTextModule,
    AdminImageUploadModule,
    AdminImageUploadComponent,
} from '../../forms/fields';

export const bazaFieldsMap = {
    AdminDateTimeDisplay: AdminDateTimeDisplayComponent,
    AdminRichText: AdminRichTextComponent,
    AdminImageUpload: AdminImageUploadComponent,
};

export const bazaFieldsComponents = [
    AdminDateTimeDisplayComponent,
    AdminRichTextComponent,
    AdminImageUploadComponent,
];

export const bazaFieldsModules = [
    AdminDateTimeDisplayModule,
    AdminRichTextModule,
    AdminImageUploadModule,
];
