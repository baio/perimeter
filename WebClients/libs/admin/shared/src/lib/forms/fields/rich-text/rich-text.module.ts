import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { AdminRichTextComponent } from './rich-text.component';

@NgModule({
    imports: [CommonModule, QuillModule, ReactiveFormsModule, FormsModule],
    declarations: [AdminRichTextComponent],
    exports: [AdminRichTextComponent],
})
export class AdminRichTextModule {
    constructor() {}
}
