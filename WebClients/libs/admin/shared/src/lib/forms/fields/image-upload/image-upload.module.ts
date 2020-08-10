import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HlcNzImageUploadModule } from '@nz-holistic/nz-controls';

import { AdminImageEditorModule } from './image-editor/image-editor.module';
import { AdminImageUploadComponent } from './image-upload.component';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        AdminImageEditorModule,
        HlcNzImageUploadModule,
    ],
    declarations: [AdminImageUploadComponent],
    exports: [AdminImageUploadComponent],
})
export class AdminImageUploadModule {}
