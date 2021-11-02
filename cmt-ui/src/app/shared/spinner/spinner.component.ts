import { Component, Input } from '@angular/core';

@Component({
    selector: 'spinner',
    template: `<div [ngClass]="size" class="progress"><div>Loading…</div></div>`,
    styleUrls: ['spinner.component.css']
})
export class SpinnerComponent {
    @Input()
    size: string;
}