import { Component, Input } from '@angular/core';
import { ImageCroppedEvent } from 'ngx-image-cropper';

export interface AdminImageEditorConfig {
    acceptWithAspectRatioOnly: boolean;
    aspectRatio: number;
    maintainAspectRatio: boolean;
    alignImage: 'center' | 'left';
}

export const bazaImageDefaultEditorConfig: AdminImageEditorConfig = {
    acceptWithAspectRatioOnly: false,
    aspectRatio: 1,
    maintainAspectRatio: false,
    alignImage: 'center',
};

const dataURLtoFile = (dataurl, filename) => {
    const arr = dataurl.split(','),
        mime = arr[0].match(/:(.*?);/)[1],
        bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);

    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }

    return new File([u8arr], filename, { type: mime });
};

@Component({
    selector: 'admin-image-editor',
    templateUrl: './image-editor.component.html',
    styleUrls: ['./image-editor.component.scss'],
})
export class AdminImageEditorComponent {
    @Input() imageFile: File;
    @Input() config = bazaImageDefaultEditorConfig;

    croppedImage: { file: File; base64: string };

    constructor() {}

    onImageCropped(event: ImageCroppedEvent) {
        this.croppedImage = {
            base64: event.base64,
            file: dataURLtoFile(event.base64, this.imageFile.name),
        };
    }
}
