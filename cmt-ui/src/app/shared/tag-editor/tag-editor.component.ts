import { Component, OnInit, Input, ElementRef, ViewChild, Output, EventEmitter } from '@angular/core';

@Component({
	selector: 'tag-editor',
	templateUrl: 'tag-editor.component.html',
	styleUrls: [ 'tag-editor.component.scss' ],
	host: {
		'(document:click)': 'onDocumentClick($event)'
	}
})
export class TagEditorComponent implements OnInit {
	editMode = false;
	@Output() removeTag = new EventEmitter();

	@Input() tags: any[];

	@Input() allowEdit: boolean;

	@ViewChild('names') namesEditArea: ElementRef;

	constructor() {}

	ngOnInit() {}

	startEditMode() {
		if (!this.allowEdit) {
			return;
		}
		this.editMode = true;
	}

	closeEditMode(event) {
		if (event) {
			event.stopPropagation();
		}
		this.editMode = false;
	}

	onRemoveTag(event, value) {
		event.stopPropagation();
		this.removeTag.emit(value);
	}

	onDocumentClick(event) {
		if (!this.editMode) {
			return;
		}

		if (!this.namesEditArea.nativeElement.contains(event.target)) {
			this.closeEditMode(null);
		}
	}
}
