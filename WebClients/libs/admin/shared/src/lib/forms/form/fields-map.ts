import {
    AdminDateTimeDisplayComponent,
    AdminDateTimeDisplayModule,
    AdminRichTextComponent,
    AdminRichTextModule,
    AdminImageUploadModule,
    AdminImageUploadComponent,
} from '../../forms/fields';

export const adminFieldsMap = {
    AdminDateTimeDisplay: AdminDateTimeDisplayComponent,
    AdminRichText: AdminRichTextComponent,
    AdminImageUpload: AdminImageUploadComponent,
};

export const adminFieldsComponents = [
    AdminDateTimeDisplayComponent,
    AdminRichTextComponent,
    AdminImageUploadComponent,
];

export const adminFieldsModules = [
    AdminDateTimeDisplayModule,
    AdminRichTextModule,
    AdminImageUploadModule,
];
