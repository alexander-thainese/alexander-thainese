import { Component, Input, ViewContainerRef } from '@angular/core';

@Component({
    selector: 'column-data-template',
    template: ``
})
export class ColumnDataTemplateComponent {
    @Input() 
    column: any;
    @Input()
    row: any;
    @Input()
    rowIndex: number;
            
    constructor(protected viewContainer: ViewContainerRef) {

    }
    
    ngOnInit() {
        let view = this.viewContainer.createEmbeddedView(this.column.dataTemplate, {
            '\$implicit': this.column,
            'row': this.row,
            'rowIndex': this.rowIndex
        });
    }
}