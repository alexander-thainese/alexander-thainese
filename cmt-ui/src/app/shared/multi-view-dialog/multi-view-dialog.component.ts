import { Component, Input, Output, EventEmitter, AfterContentInit, ContentChildren, QueryList } from '@angular/core';

import { DialogViewComponent } from './dialog-view/dialog-view.component'

@Component({
    selector: 'multi-view-dialog',
    templateUrl: 'multi-view-dialog.component.html',
    styleUrls: ['multi-view-dialog.component.css']
})
export class MultiViewDialogComponent implements AfterContentInit {
    @Input()
    title: string;
    @Input()
    subtitle: string;
    @Input()
    buttonPreviousText: string;
    @Input()
    buttonNextText: string;
    @Input()
    buttonPreviousVisible: boolean;
    @Input()
    buttonNextVisible: boolean;
    @Input()
    initialViewName: string = null;
    @Input()
    forceHideButtonPrevious: boolean = false;
    @Input()
    forceHideButtonNext: boolean = false;

    @ContentChildren(DialogViewComponent)
    views: QueryList<DialogViewComponent>;
    activeViewIndex: number;

    constructor() {
    }

    ngAfterContentInit() {
        for (let i = 0; i < this.views.toArray().length; i++) {
            this.views.toArray()[i].nextClick.subscribe(o => this.nextView());
            this.views.toArray()[i].previousClick.subscribe(o => this.previousView());
        }

        if (this.initialViewName) {
            let index = this.views.toArray().findIndex(o => o.name == this.initialViewName);

            if (index != null)
                this.activateView(index, this.forceHideButtonPrevious, this.forceHideButtonNext);
            else
                this.activateView(0);
        }
        else {
            this.activateView(0);
        }
    }

    nextView(hideButtonPrevious: boolean = null, hideButtonNext: boolean = null) {
        this.activateView(this.activeViewIndex + 1, hideButtonPrevious, hideButtonNext);
    }

    previousView(hideButtonPrevious: boolean = null, hideButtonNext: boolean = null) {
        this.activateView(this.activeViewIndex - 1, hideButtonPrevious, hideButtonNext);
    }

    private activateView(index: number, hideButtonPrevious: boolean = null, hideButtonNext: boolean = null) {
        if (this.activeViewIndex != null)
            this.views.toArray()[this.activeViewIndex].active = false;

        this.activeViewIndex = index;
        this.views.toArray()[index].active = true;
        this.views.toArray()[index].buttonPreviousVisible = hideButtonPrevious == null ? !this.isFirstView(index) : !this.isFirstView(index) && !hideButtonPrevious;
        this.views.toArray()[index].buttonNextVisible = hideButtonNext == null ? !this.isLastView(index) : !this.isLastView(index) && !hideButtonNext;
    }

    private isFirstView(viewIndex: number): boolean {
        return viewIndex == 0;
    }

    private isLastView(viewIndex: number): boolean {
        return viewIndex == this.views.toArray().length - 1;
    }
}