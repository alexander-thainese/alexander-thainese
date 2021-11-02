import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'dialog-view',
    templateUrl: 'dialog-view.component.html',
    styleUrls: ['dialog-view.component.css']
})
export class DialogViewComponent {
    @Input()
    active: boolean;
    @Input()
    name: string;
    @Input()
    caption: string;
    @Input()
    description: string;
    @Input()
    okText: string = 'Accept';
    @Input()
    cancelText: string = 'Cancel';
    @Input()
    buttonNextText: string;
    @Input()
    buttonPreviousText: string;
    @Input()
    buttonOkVisible: boolean = true;
    @Input()
    buttonCancelVisible: boolean = true;
    @Input()
    buttonPreviousVisible: boolean;
    @Input()
    buttonNextVisible: boolean;
    @Output()
    previousClick: EventEmitter<any> = new EventEmitter<any>();
    @Output()
    nextClick: EventEmitter<any> = new EventEmitter<any>();
    @Output()
    okClick: EventEmitter<any> = new EventEmitter();
    @Output()
    cancelClick: EventEmitter<any> = new EventEmitter();

    selectedValue: any;

    private onPreviousClick() {
        this.previousClick.emit(null);
    }

    private onNextClick() {
        this.nextClick.emit(null);
    }

    private onOkClick() {
        this.okClick.emit(this.selectedValue);
    }

    private onCancelClick() {
        this.cancelClick.emit(null);
    }
}