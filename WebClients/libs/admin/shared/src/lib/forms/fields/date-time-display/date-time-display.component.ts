import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
    selector: 'admin-date-time-display',
    templateUrl: './date-time-display.component.html',
    styleUrls: ['./date-time-display.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => AdminDateTimeDisplayComponent),
            multi: true
        },
        DatePipe
    ]
})
export class AdminDateTimeDisplayComponent implements OnInit, ControlValueAccessor {
    constructor(private readonly cdr: ChangeDetectorRef, datePipe: DatePipe) {
        this.format = (x: string) => {
            if (!x) {
                return '';
            }
            return datePipe.transform(x, 'medium') || '';
        };
    }
    @Input()
    value: string | undefined;

    @Input()
    readonly: boolean;

    readonly modules: any;

    format: any;
    propagateChange = (_: any) => {};

    ngOnInit() {}

    onModelChange(event: any) {
        this.value = event;
        this.propagateChange(this.value);
    }

    //

    writeValue(obj: any) {
        this.value = obj;
        this.cdr.markForCheck();
    }

    registerOnChange(fn: any) {
        this.propagateChange = fn;
    }

    registerOnTouched(_: any) {}
}
