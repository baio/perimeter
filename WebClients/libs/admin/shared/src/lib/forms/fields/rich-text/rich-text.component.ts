import { AfterContentChecked, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Inject, InjectionToken, Input, OnInit, Optional, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { QuillEditorComponent } from 'ngx-quill';
import Quill from 'quill';

// TODO :
// import ImageResize from 'quill-image-resize-module';
// import ImageUpload from 'quill-plugin-image-upload';

// Quill.register('modules/imageUpload', ImageUpload);
// Quill.register('modules/imageResize', ImageResize);

export type AdminRichTextFileUploader = (file: File) => Promise<string>;

export const ADMIN_RICH_TEXT_FILE_UPLOADER = new InjectionToken<AdminRichTextFileUploader>(
    'ADMIN_RICH_TEXT_FILE_UPLOADER'
);

/**
 * Small wrapper around https://github.com/KillerCodeMonkey/ngx-quill
 * Issues:
 * https://github.com/KillerCodeMonkey/ngx-quill/issues/210
 * https://github.com/KillerCodeMonkey/ngx-quill/issues/273
 * https://github.com/quilljs/quill/issues/2190
 */
@Component({
    selector: 'admin-rich-text',
    templateUrl: './rich-text.component.html',
    styleUrls: ['./rich-text.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => AdminRichTextComponent),
            multi: true,
        },
    ],
})
export class AdminRichTextComponent implements OnInit, ControlValueAccessor, AfterContentChecked {
    // See issues above
    _delayedValue: string;
    wasElementVisible = false;

    @Input()
    style: { [key: string]: any };

    @Input()
    placeholder: string;

    @Input()
    value: string | undefined;

    @Input()
    readonly: boolean;

    @ViewChild(QuillEditorComponent, { static: true }) editor: QuillEditorComponent;

    readonly modules: any;
    propagateChange = (_: any) => {};

    constructor(
        private readonly cdr: ChangeDetectorRef,
        private readonly elementRef: ElementRef,
        @Optional() @Inject(ADMIN_RICH_TEXT_FILE_UPLOADER) private readonly fileUploader?: AdminRichTextFileUploader
    ) {
        const imageUpload = this.fileUploader && {
            upload: (file) => {
                return this.fileUploader(file);
            },
        };

        this.modules = {
            // imageResize: true,
            // imageUpload,
            toolbar: [
                [{ header: [1, 2, 3, 4, 5, 6, false] }],
                ['bold', 'italic', 'link', { list: 'ordered' }, { list: 'bullet' }, 'image', 'blockquote'],
            ],
        };
    }

    ngOnInit() {}

    ngAfterContentChecked(): void {
        if (!this.wasElementVisible) {
            const isVisible = !!this.elementRef.nativeElement.offsetParent;
            if (isVisible) {
                this.wasElementVisible = true;
                this.value = this._delayedValue;
                this._delayedValue = null;
                this.cdr.markForCheck();
            }
        }
    }

    onModelChange(event: any) {
        this.value = event;
        this.propagateChange(this.value);
    }

    //

    writeValue(obj: any) {
        if (this.wasElementVisible) {
            this.value = obj;
            this.cdr.markForCheck();
        } else {
            this._delayedValue = obj;
        }
    }

    registerOnChange(fn: any) {
        this.propagateChange = fn;
    }

    registerOnTouched(_: any) {}
}
