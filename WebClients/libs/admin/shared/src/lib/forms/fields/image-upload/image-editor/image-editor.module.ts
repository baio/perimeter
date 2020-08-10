import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ImageCropperModule } from 'ngx-image-cropper';

import { AdminImageEditorComponent } from './image-editor.component';

@NgModule({
    imports: [CommonModule, ImageCropperModule],
    declarations: [AdminImageEditorComponent],
    exports: [AdminImageEditorComponent],
})
export class AdminImageEditorModule {}
