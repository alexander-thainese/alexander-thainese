import { Component, OnInit, ViewChild, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { TreeViewService, MetadataService, SchemaElement, ItemType, ItemState } from '../index';
import { MatIconRegistry } from '@angular/material/icon';
import { CmtElementDetailComponent } from './element-detail';
import { OVERLAY_PROVIDERS } from '@angular/cdk/overlay';
import { DialogComponent } from '../dialog';
import { HttpClient } from '../http-client.service';
import { ParentValueAttribute } from '../parent-value-attribute';

@Component({
	selector: 'cmt-tree-view',
	templateUrl: 'tree-view.component.html',
	viewProviders: [ MatIconRegistry ],
	providers: [ OVERLAY_PROVIDERS ]
})
export class CmtTreeViewComponent implements OnInit, OnChanges {
	@Input() readonly = true;
	@Input() hideValueDetails = false;
	@Output() refreshData = new EventEmitter<string>();
	@Output() selectionChange = new EventEmitter<SchemaElement>();
	@Input() data: SchemaElement[] = [];
	@Input() errorMessage: string;
	@Input() title: string;
	@Input() loading: boolean;
	@Input() showFilter = false;
	@Input() showAddIcon = false;
	@Input() showContent = false;
	@Input() lazyLoad = false;
	@Output() addItem = new EventEmitter();
	@Output() filter = new EventEmitter<boolean>();
	itemType = ItemType;
	selectedObject: SchemaElement;

	@Input() filterMessage = '';

	isFilterActive = true;

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient
	) {
		treeViewService.selection$.subscribe((item) => {
			this.updateSelection(item);
		});
	}

	onSearch(searchTerm: string) {
		this.refreshData.emit(searchTerm.trim());
	}

	updateSelection(selectedItem: SchemaElement): void {
		if (selectedItem && !selectedItem.Name && selectedItem.Id !== 'newid') {
			this.selectedObject = this.data.find((p) => p.Id === selectedItem.Id);
		} else {
			this.selectedObject = selectedItem;
		}
		if (this.selectedObject && !this.selectedObject.ParentValuesAttributes) {
			this.metadataService
				.getAllParentValuesAttribute()
				.then((res) => (this.selectedObject.ParentValuesAttributes = res as ParentValueAttribute[]));
		}
		this.selectionChange.emit(selectedItem);
	}
	updateData() {
		this.refreshData.emit();
	}

	ngOnInit() {
		this.filter.emit(this.isFilterActive);
	}

	get allowAddNew(): boolean {
		return this.http.isAdmin;
	}

	showDialog(item?: any) {
		this.addItem.emit(item);
	}

	ngOnChanges(changes: SimpleChanges) {
		let c = changes as any;
	}

	toggleFilter() {
		this.isFilterActive = !this.isFilterActive;
		this.filter.emit(this.isFilterActive);
	}

	addElement(element: SchemaElement) {
		this.addItem.emit(element);
	}
}
