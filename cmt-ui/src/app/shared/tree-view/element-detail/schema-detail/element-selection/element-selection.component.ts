import { distinctUntilChanged, debounceTime } from 'rxjs/operators';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { Subject } from 'rxjs';
import { MetadataService } from './../../../../metadata.service';
import { Component, OnInit, OnChanges, SimpleChanges, Input, ViewChild, EventEmitter, Output } from '@angular/core';
import { SchemaElement } from '../../../../schema-element';

@Component({
	selector: 'schema-element-selection',
	templateUrl: 'element-selection.component.html',
	styleUrls: [ 'element-selection.component.scss' ]
})
export class SchemaElementSelectionComponent implements OnInit {
	data: any[];
	selectedItems: { id: string; selected: boolean }[] = [];
	unfilteredData: any[];
	_searchTerm: string = '';
	searchTerms = new Subject<string>();
	refreshRequests = new Subject<any>();
	isFilterActive: boolean = false;
	filterMessage: string = '';
	@Input() schemaId: string = '';
	@Output() closeEdit = new EventEmitter<boolean>();
	editLovPoupVisible: boolean = false;
	defaultvalueOnPopup: string = null;
	isLovOnPopup: boolean;
	dropdownItems = [];
	editElement: any;
	requiredOnPopup: boolean;
	notDefinedValue = null;

	constructor(private metadataService: MetadataService) {}

	ngOnInit() {
		this.searchTerms.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.filterData(term));
		this.refreshRequests.pipe(debounceTime(1000)).subscribe((term) => {
			this.refreshData(true);
		});
		this.refreshData(false);
	}

	refreshData(filterdata: boolean) {
		this.metadataService.getMetadataElementsWithDescription(this.schemaId).then((data) => {
			this.data = data as SchemaElement[];
			this.unfilteredData = data as SchemaElement[];
			this.selectedItems = this.data.map((p) => {
				return { id: p.Id, selected: p.Status };
			});
			if (filterdata) {
				this.filterData(this.searchTerm);
			}
		});
	}

	set searchTerm(value: string) {
		this._searchTerm = value;
		this.searchTerms.next(value);
	}
	get searchTerm(): string {
		return this._searchTerm;
	}

	filterData(searchTerm: string) {
		this.data = this.unfilteredData.filter(
			(p) =>
				p.Name.toLowerCase().indexOf(this._searchTerm.toLowerCase()) > -1 &&
				(p.IsLov === true || !this.isFilterActive)
		);
	}
	checkedChanged(event: MatCheckboxChange, el, checkbox: MatCheckbox) {
		if (el.InProgress) {
			event.source.checked = !event.checked;
			return;
		}
		el.InProgress = true;
		if (event.checked) {
			this.metadataService
				.addElementToSchema(this.schemaId, el.Id)
				.then((res) => {
					this.refreshRequests.next(null);
					el.InProgress = false;
				})
				.catch((res) => {
					el.InProgress = false;
					el.Status = 0;
				});
		} else {
			this.metadataService
				.deleteElementFromSchema(this.schemaId, el.Id)
				.then((res) => {
					this.refreshRequests.next(null);
					el.InProgress = false;
				})
				.catch((res) => {
					el.InProgress = false;
					checkbox.disabled = false;
					el.Status = 1;
				});
		}
	}

	back() {
		this.closeEdit.emit(false);
	}

	toggleFilter() {
		this.isFilterActive = !this.isFilterActive;
		this.filterMessage = 'Show all elements';
		if (!this.isFilterActive) {
			this.filterMessage = 'Show only LOVs';
		}

		this.filterData(this.searchTerm);
	}

	edit(el) {
		this.editElement = el;
		this.editLovPoupVisible = true;
		this.isLovOnPopup = el.IsLov;
		this.requiredOnPopup = el.IsRequired;

		this.dropdownItems = [];
		this.metadataService.getElementValuesForDefault(el.Id).then((v) => {
			this.dropdownItems = v as any[];
			this.defaultvalueOnPopup = el.DefaultValue.Value;
		});
	}

	saveSchemaElement(val) {
		if (val) {
			let result = { DefaultValue: this.defaultvalueOnPopup, IsRequired: this.requiredOnPopup };
			this.metadataService.setSchemaElementProperties(this.schemaId, this.editElement.Id, result).then((el) => {
				this.refreshRequests.next(null);
			});
		}
		this.defaultvalueOnPopup = null;
		this.dropdownItems = [];

		this.editLovPoupVisible = false;
	}
}
