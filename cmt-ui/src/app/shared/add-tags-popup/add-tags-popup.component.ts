import { Component, OnInit, Output, EventEmitter, ViewChild, ElementRef, Input, OnDestroy } from '@angular/core';

@Component({
	selector: 'app-add-tags-popup',
	templateUrl: './add-tags-popup.component.html',
	styleUrls: [ './add-tags-popup.component.scss' ]
})
export class AddTagsPopupComponent implements OnInit, OnDestroy {
	@ViewChild('inputText') inputText: ElementRef;

	@Output() hidePopup = new EventEmitter();
	@Output() onSave = new EventEmitter();
	@Output() onClickTag: EventEmitter<any> = new EventEmitter();
	@Output() onRemoveTag: EventEmitter<any> = new EventEmitter();
	@Output() onInputChange = new EventEmitter();
	@Output() onEnter: EventEmitter<any> = new EventEmitter();
	@Output() onClickBeyond: EventEmitter<any> = new EventEmitter();

	public inputTag;
	@Input() public tags = [];

	@Input() public allowCustomTags = false;
	@Input() public displayNameField = 'name';

	constructor() {}

	onChange(inputText): void {
		this.onInputChange.emit(inputText);
	}

	onClick(nameOfTag) {
		this.onClickTag.emit(nameOfTag);
		this.inputText.nativeElement.focus();
		this.inputTag = '';
	}

	onEnterKey(inputText, lastTag, event: KeyboardEvent) {
		event.preventDefault();
		let type;
		if (this.tags.length === 1) {
			if (this.tags[0].name.toLowerCase() === inputText.toLowerCase()) {
				this.onClickTag.emit(this.tags[0]);
			}
		} else if (this.allowCustomTags === true) {
			this.onEnter.emit(inputText);
		}
		this.inputTag = '';
	}

	_onClickBeyond() {
		this.onClickBeyond.emit();
	}
	clickEventHandler(event) {
		event.stopPropagation();
	}
	offClickHandler() {
		this._onClickBeyond();
	}
	ngOnInit() {
		this.inputText.nativeElement.focus();
		setTimeout(() => {
			document.addEventListener('click', this.offClickHandler.bind(this));
		}, 100);
	}
	ngOnDestroy() {
		document.removeEventListener('click', this.offClickHandler);
	}
}
