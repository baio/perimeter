import {
    Component,
    EventEmitter,
    forwardRef,
    Input,
    OnInit,
    Output,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {
    HlcNzFileUploadDataAccess,
    HlcNzFileUploadMode,
} from '@nz-holistic/nz-controls';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzUploadFile } from 'ng-zorro-antd/upload';
import { Observable } from 'rxjs';
import {
    AdminImageEditorComponent,
    bazaImageDefaultEditorConfig,
} from './image-editor/image-editor.component';

@Component({
    selector: 'admin-image-upload',
    templateUrl: './image-upload.component.html',
    styleUrls: ['./image-upload.component.scss'],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => AdminImageUploadComponent),
            multi: true,
        },
    ],
})
export class AdminImageUploadComponent implements OnInit, ControlValueAccessor {
    beforeUpload: (file: NzUploadFile) => Observable<NzUploadFile | false>;

    @Input() value: string | string[];

    // If this param is defined,  before image upload will be shown modal where user can crop image size
    @Input() editorConfig = bazaImageDefaultEditorConfig;
    @Input() accept = 'image/png,image/jpeg,image/gif,image/bmp';
    @Input() dataAccess: HlcNzFileUploadDataAccess;
    @Input() id: string;
    @Input() mode: HlcNzFileUploadMode = 'single';
    @Input() readonly: boolean | undefined;
    @Input() allowOrderItems: boolean | undefined;

    @Output() valueChange = new EventEmitter<string | string[]>();

    propagateChange = (_: any) => {};

    constructor(private readonly modalService: NzModalService) {}

    ngOnInit() {
        if (this.editorConfig) {
            this.beforeUpload = (file: NzUploadFile) =>
                new Observable((resolver) => {
                    this.modalService.create({
                        nzTitle: 'Edit Image',
                        nzContent: AdminImageEditorComponent,
                        nzComponentParams: {
                            imageFile: file as any,
                            config: this.editorConfig,
                        },
                        nzOnOk: (inst) => {
                            const croppedImage = inst.croppedImage;
                            const updFile: NzUploadFile = {
                                ...file,
                                originFileObj: croppedImage.file,
                                thumbUrl: croppedImage.base64,
                            };
                            resolver.next(updFile);
                            resolver.complete();
                        },
                        nzOnCancel: () => {
                            resolver.next(false);
                            resolver.complete();
                        },
                    });
                });
        }
    }

    onValueChange(obj: string | string[]) {
        this.value = obj;
        this.propagateChange(this.value);
    }

    writeValue(obj: any) {
        this.value = obj;
    }

    registerOnChange(fn: any) {
        this.propagateChange = fn;
    }

    registerOnTouched(_: any) {}
}
