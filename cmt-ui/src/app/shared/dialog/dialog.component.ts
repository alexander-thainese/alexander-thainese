import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { MatInput } from '@angular/material/input';

@Component({
	selector: 'cmt-dialog',
	templateUrl: 'dialog.component.html',
	styleUrls: [ 'dialog.component.css' ]
})
export class DialogComponent implements OnInit {
	@ViewChild(MatInput) input: ElementRef<MatInput>;
	@Input() value: string;
	@Input() placeholder: string;
	@Input() title: string;
	@Input() template: string;
	@Input() okText: string;
	@Input() cancelText: string;
	@Output() valueEmitted = new EventEmitter<string>();
	_showPrompt: boolean;

	constructor() {
		this.okText = 'OK';
		this.cancelText = 'Cancel';
	}

	get showPrompt(): boolean {
		return this._showPrompt;
	}
	@Input()
	set showPrompt(v: boolean) {
		this._showPrompt = v;
	}

	emitValue(value) {
		if (!value) {
			this.value = '';
		}
		this.valueEmitted.emit(value);
	}

	keypressHandler(event: any) {
		if (event.charCode === 13) {
			if (document.activeElement !== document.body) {
				let element = document.activeElement as any;
				element.blur();
				this.valueEmitted.emit(this.value);
			}
			return false;
		}
		return true;
	}

	ngOnInit() {}
}
