import { Component, OnInit, Input, Output, forwardRef, ViewChild, EventEmitter, ElementRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { MatInput } from '@angular/material/input';
import { MatIconRegistry } from '@angular/material/icon';

export const INLINE_VALUE_ACCESSOR: any = {
	provide: NG_VALUE_ACCESSOR,
	useExisting: forwardRef(() => CmtInlineSearch),
	multi: true
};

@Component({
	selector: 'cmt-inline-search',
	styleUrls: [ 'inline-search.component.css' ],
	templateUrl: 'inline-search.component.html',
	providers: [ INLINE_VALUE_ACCESSOR ],
	viewProviders: [ MatIconRegistry ]
})
export class CmtInlineSearch implements OnInit, ControlValueAccessor {
	// get accessor
	get value(): any {
		return this._value;
	}
	// set accessor including call the onchange callback
	@Input()
	set value(v: any) {
		if (v !== this._value && (v.length >= this.minLength || v.length === 0)) {
			this._value = v;
			this.search.emit(v);
			this.onModelChange(v);
		}
	}

	get showSearchBox() {
		return this.searchMode || (this.input && this.input.value && this.input.value.length > 0);
	}
	_value: any;
	@Input() displayText: string;
	@Input() nullValueText = 'Enter value';

	@Output() search = new EventEmitter<string>();

	@Input() readonly = false;
	@Input() minLength = 1;

	searchMode = false;
	@ViewChild(MatInput) input: MatInput;
	valueChanged: boolean;

	constructor() {
		// this.editMode = false;
	}

	onModelChange: Function = () => {};
	onModelTouched: Function = () => {};

	// public get displayMode() { return !this.searchMode; }

	public onClick() {
		if (this.readonly) {
			return;
		}
		this.searchMode = true;
		setTimeout(() => {
			this.input.focus();
		}, 800);
	}

	public onBlur() {
		this.searchMode = false;
	}

	public onFocus() {
		this.searchMode = true;
	}

	eventHandler(event: any) {
		if (event.charCode === 13) {
			this.value = this.input.value;
			return false;
		}
		return true;
	}

	// From ControlValueAccessor interface
	writeValue(value: any) {
		if (value !== this._value) {
			this._value = value;
		}
	}

	registerOnChange(fn: Function): void {
		this.onModelChange = fn;
	}

	registerOnTouched(fn: Function): void {
		this.onModelTouched = fn;
	}

	ngOnInit() {}
}
