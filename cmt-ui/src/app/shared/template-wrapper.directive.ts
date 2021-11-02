import { Directive, ViewContainerRef, OnInit, Input, TemplateRef } from '@angular/core';

@Directive({
    selector: '[TemplateWrapper]'
})
export class TemplateWrapper implements OnInit {
    @Input() 
    item: any;
    @Input('TemplateWrapper')
    templateRef: TemplateRef<any>;
    
    constructor(protected viewContainer: ViewContainerRef) {

    }
    
    ngOnInit() {
        let view = this.viewContainer.createEmbeddedView(this.templateRef, {
            '\$implicit': this.item
        });
    }
}