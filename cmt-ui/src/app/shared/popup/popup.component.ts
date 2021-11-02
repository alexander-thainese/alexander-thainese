import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { MatInput } from '@angular/material/input';
import { PopupListService } from './popup.service';

@Component({
	selector: 'cmt-popup',
	templateUrl: 'popup.component.html',
	styleUrls: [ 'popup.component.css' ]
})
export class PopupComponent implements OnInit {
	@ViewChild(MatInput) input: ElementRef<MatInput>;
	@Input() value: string;
	@Input() placeholder: string;
	@Input() title: string;
	@Input() template: string;
	@Input() okText: string;
	@Input() cancelText: string;
	@Input() okButtonDisabled = false;
	@Output() valueEmitted = new EventEmitter<string>();
	_showPrompt: boolean;

	constructor(private popupListService: PopupListService) {
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
			value = '';
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
