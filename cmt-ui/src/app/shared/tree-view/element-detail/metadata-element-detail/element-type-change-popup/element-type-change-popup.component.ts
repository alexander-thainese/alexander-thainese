import { Component, Input, ChangeDetectionStrategy, OnInit, EventEmitter, Output } from '@angular/core';
import { MetadataService } from '../../../../metadata.service';

@Component({
	selector: 'element-type-change-popup',
	templateUrl: 'element-type-change-popup.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class ElementTypeChangePopupComponent implements OnInit {
	@Input() elementTypeId: string;
	@Input() visible = false;

	elementTypes: any[] = [];
	@Output() change = new EventEmitter<string>();

	constructor(private readonly metadataService: MetadataService) {}

	ngOnInit(): void {
		this.getElementTypes();
	}

	getElementTypes() {
		if (this.elementTypes.length === 0) {
			this.metadataService.getMetadataElementTypes().subscribe((p) => {
				this.elementTypes = p;
			});
		}
	}

	changeElementType(event: any) {
		let elementTypeId = '';
		if (event === true) {
			elementTypeId = this.elementTypeId;
		}

		this.change.emit(elementTypeId);
	}
}
