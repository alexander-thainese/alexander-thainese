import { Component, Input, ViewContainerRef } from '@angular/core';

@Component({
    selector: 'column-header-template',
    template: ``
})
export class ColumnHeaderTemplateComponent {
    @Input() 
    column: any;
            
    constructor(protected viewContainer: ViewContainerRef) {
        
    }
    
    ngOnInit() {
        let view = this.viewContainer.createEmbeddedView(this.column.headerTemplate, {
            '\$implicit': this.column
        });
    }
}