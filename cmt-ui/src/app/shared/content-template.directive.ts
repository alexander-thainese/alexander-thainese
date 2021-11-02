import { Directive, Input, TemplateRef } from '@angular/core';

@Directive({
    selector: '[contentTemplate]',
    host: {}
})
export class ContentTemplateDirective {
    @Input()
    type: string;

    constructor(protected template: TemplateRef<any>) {

    }
}