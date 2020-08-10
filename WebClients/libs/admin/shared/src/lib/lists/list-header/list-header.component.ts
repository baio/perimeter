import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HlcNzTableFilterService } from '@nz-holistic/nz-list';

@Component({
    selector: 'admin-list-header',
    templateUrl: './list-header.component.html',
    styleUrls: ['./list-header.component.scss'],
})
export class AdminListHeaderComponent implements OnInit {
    @Input()
    canAdd = true;

    @Input()
    hasBackButton = false;

    @Input()
    searchPlaceholder = 'Search';

    @Input()
    subTitle: string;

    @Input()
    title: string;

    readonly filterForm: FormGroup;

    constructor(
        fb: FormBuilder,
        private readonly activatedRoute: ActivatedRoute,
        private readonly filterService: HlcNzTableFilterService,
        private readonly router: Router
    ) {
        this.filterForm = fb.group({
            text: [null],
        });
    }

    ngOnInit() {}

    // onAdd
    onAdd() {
        this.router.navigate(['.', 'new'], { relativeTo: this.activatedRoute });
    }

    // onBack
    onBack() {
        this.router.navigate(['.'], {
            relativeTo: this.activatedRoute.parent.parent,
        });
    }

    // onFilter
    onFilter() {
        const value = this.filterForm.value;
        const text = value.text || null;

        this.filterService.setValue(text ? { text } : 'none');
    }
}
