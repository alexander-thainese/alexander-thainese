import { MatSelect } from '@angular/material/select';
import {
	Component,
	OnInit,
	Input,
	Output,
	forwardRef,
	ViewChild,
	EventEmitter,
	ChangeDetectionStrategy,
	ElementRef
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { MatInput } from '@angular/material/input';

export const INLINE_VALUE_ACCESSOR: any = {
	provide: NG_VALUE_ACCESSOR,
	// tslint:disable-next-line: no-use-before-declare
	useExisting: forwardRef(() => CmtInlineEditComponent),
	multi: true
};

@Component({
	selector: 'cmt-inline-edit',
	styleUrls: [ 'inline-edit.component.css' ],
	templateUrl: 'inline-edit.component.html',
	providers: [ INLINE_VALUE_ACCESSOR ]
})
export class CmtInlineEditComponent implements OnInit, ControlValueAccessor {
	// get accessor
	get value(): any {
		return this._value;
	}
	// set accessor including call the onchange callback
	@Input()
	set value(v: any) {
		if (v !== this._value) {
			this._value = v;
			if (this.isRequired && (!v || v == null || v === '')) {
				this.isValid = false;
			} else {
				this.isValid = true;
				if (this.type === 'dropdown') {
					this.displayText = this.value
						? this.dropdownItems.find((p) => p.Value === v)[this.textField]
						: 'No default';
				}
			}
			this.onModelChange(v);
		}
	}

	public get displayMode() {
		return !this.editMode;
	}
	_value: any;
	@Input() displayText: string;
	@Input() nullValueText = 'Enter value';

	@Input() type = 'textbox';

	@Input() textField: string;
	@Input() valueField = 'Value';

	@Input() dropdownItems: any[] = [];

	@Input() readonly = false;

	editMode = false;
	@ViewChild('matInput') input: ElementRef;
	@ViewChild('matTextarea') matTextarea: ElementRef;
	@ViewChild('matSelect') matSelect: MatSelect;

	isValid = true;
	originalValue: any;
	valueChanged: boolean;
	@Input() isRequired = true;

	@Output() valueChange = new EventEmitter<any>();

	constructor() {}

	onModelChange: Function = () => {};
	onModelTouched: Function = () => {};

	// From ControlValueAccessor interface
	writeValue(value: any) {
		if (value !== this._value) {
			this.originalValue = value;
			this._value = value;
		}
	}

	registerOnChange(fn: Function): void {
		this.onModelChange = fn;
	}

	registerOnTouched(fn: Function): void {
		this.onModelTouched = fn;
	}

	onChange() {
		if (this.isValid) {
			// this.originalValue = this.value;
			this.valueChange.emit(this.value);
		} else {
			this.value = this.originalValue;
			this.isValid = true;
		}
	}

	public onClick() {
		if (this.readonly) {
			return;
		}
		this.editMode = true;
		switch (this.type) {
			case 'textarea':
				setTimeout(() => {
					this.matTextarea.nativeElement.focus();
				}, 800);
				break;
			case 'dropdown':
				setTimeout(() => {
					this.matSelect.open();
				}, 500);
				// this.dropdownItems = [{ Name: 'test', Value: 1 }, { Name: 'test 2', Value: 2 }];
				break;
			default:
				setTimeout(() => {
					this.input.nativeElement.focus();
				}, 500);
				break;
		}
		// setTimeout(() => { this.mdSelect.open(); }, 500);
		this.originalValue = this.value;
	}

	public onBlur() {
		this.editMode = false;
	}

	public restoreValue() {
		this.value = this.originalValue;
		this.isValid = true;
	}

	eventHandler(event: any) {
		if (event.charCode === 13) {
			this.value = this.input.nativeElement.value;
			this.editMode = false;
			if (document.activeElement !== document.body) {
				const element = document.activeElement as any;
				element.blur();
			}
			return false;
		}
		return true;
	}

	ngOnInit() {}
}
