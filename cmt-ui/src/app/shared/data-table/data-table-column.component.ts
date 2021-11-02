import { Component, Input, AfterContentInit, QueryList, ContentChildren, TemplateRef } from '@angular/core';

import { ContentTemplateDirective } from '../content-template.directive'

@Component({
    selector: 'data-table-column',
    template: ``
})
export class DataTableColumnComponent implements AfterContentInit {
    @Input()
    field: string;
    @Input()
    caption: string;
    @Input()
    width: string;
    @Input()
    visible: boolean;
    @Input()
    sortable: boolean;
    @ContentChildren(ContentTemplateDirective)
    templates: QueryList<any>;

    protected headerTemplate: TemplateRef<any>;
    protected dataTemplate: TemplateRef<any>;

    constructor() {
        this.visible = true;
    }
 
    ngAfterContentInit() {
        this.templates.forEach((item) => {
            switch (item.type) {
                case 'header':
                    this.headerTemplate = item.template;
                    break;

                case 'data':
                    this.dataTemplate = item.template;
                    break;

                default:
                    this.dataTemplate = item.template;
                    break;
            }
        });
    }
}