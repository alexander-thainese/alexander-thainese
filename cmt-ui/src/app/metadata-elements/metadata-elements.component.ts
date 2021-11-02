import { DialogComponent } from './../shared/dialog/dialog.component';
import { ItemState } from './../shared/item-state';
import { CmtTreeViewComponent } from './../shared/tree-view/tree-view.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { TreeViewService, MetadataService, SchemaElement, ItemType } from '../shared';
import { HttpClient } from '../shared/http-client.service';
import { checkFlag } from '../shared/common.functions';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
	selector: 'metadata-elements',
	templateUrl: 'metadata-elements.component.html',
	providers: [ TreeViewService, MetadataService ]
})
export class CmtMetadataElementsComponent implements OnInit {
	public itemType = ItemType;
	data: SchemaElement[];
	notFilteredData: SchemaElement[];
	elementTypes: any[] = [];
	errorMessage: string;
	loading: boolean;
	searchTerm: string;
	isFilterActive: boolean;
	@ViewChild(CmtTreeViewComponent) cmtTreeView: CmtTreeViewComponent;
	@ViewChild(DialogComponent) dialog: DialogComponent;
	dialogVisible: boolean;
	newElementName: string;
	elementType: string;
	allowElementTypeSelection: boolean;
	sourceElementId: string;
	createElementTitle: string;
	selectedObject: SchemaElement;
	showElementDetails = true;
	removeDerivedConfirmationPopupVisible = false;
	confirmElementDeactivationPopupVisible = false;
	manageTagsVisible: boolean;
	allowRemoveDerived$: Observable<boolean>;

	get allowNewDerived(): boolean {
		return (
			this.http.isAdmin &&
			this.selectedObject &&
			this.selectedObject.IsLov &&
			this.selectedObject.RootId === this.selectedObject.Id &&
			!checkFlag(this.selectedObject.Attributes, 4)
		);
	}

	get allowRemoveDerived(): boolean {
		return (
			this.http.isAdmin &&
			this.selectedObject &&
			this.selectedObject.IsLov &&
			checkFlag(this.selectedObject.Attributes, 4)
		);
	}

	get allowDownload(): boolean {
		return this.http.isAdmin;
	}

	get allowDeactivate(): boolean {
		return this.http.isAdmin && this.selectedObject && this.selectedObject.Status;
	}

	get elementDetailsVisible(): boolean {
		return this.showElementDetails && this.selectedObject.Type === ItemType.Element;
	}

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient
	) {
		this.refreshData('');
		treeViewService.refreshData$.subscribe((searchTerm) => {
			this.refreshData(searchTerm);
		});
	}
	ngOnInit() {}

	refreshData(searchTerm: string) {
		if (searchTerm != null) {
			this.searchTerm = searchTerm;
		}
		this.loading = true;
		this.metadataService.getMetadataElements(this.searchTerm).then(
			(elements) => {
				this.data = elements as SchemaElement[];
				this.notFilteredData = this.data;
				this.onFilterByType(this.isFilterActive);
				this.errorMessage = null;
				this.loading = false;
			},
			(error) => {
				this.errorMessage = error;
				this.loading = false;
			}
		);
	}

	onFilterByType(isActive: boolean) {
		this.isFilterActive = isActive;
		if (isActive) {
			this.cmtTreeView.filterMessage = 'Show all elements';
			if (this.data) {
				this.data = this.data.filter((p) => p.IsLov);
			}
		} else {
			this.data = this.notFilteredData;
			this.cmtTreeView.filterMessage = 'Show only LOVs';
		}
	}

	toggleElementDetails() {
		this.showElementDetails = !this.showElementDetails;
	}

	addElement(value) {
		if (value) {
			this.newElementName = this.newElementName.trim();
			let element = new SchemaElement();
			element.Name = this.newElementName;
			element.TypeId = this.elementType;
			element.SourceElementId = this.sourceElementId;
			this.metadataService.addElement(element).then(
				(result) => {
					if (result) {
						let selectedIndex = this.treeViewService.ItemsState.findIndex((p) => p.isSelected);
						this.treeViewService.ItemsState[selectedIndex].isSelected = false;
						this.treeViewService.ItemsState.push(new ItemState(result, true));
						this.treeViewService.refreshData('');
					}
				},
				(error) => {
					alert('error');
				}
			);
		}
		this.newElementName = '';
		this.hideDialog();
	}

	get canCreate(): boolean {
		return !!this.newElementName && !!this.elementType;
	}

	showDialog(source?: SchemaElement) {
		this.allowElementTypeSelection = !source;
		if (source) {
			this.createElementTitle = 'Create dependent metadata element from ' + source.Name;
			this.sourceElementId = source.Id;
		} else {
			this.sourceElementId = undefined;
			this.createElementTitle = 'Create new metadata element';
		}
		this.dialogVisible = true;
		if (this.elementTypes.length === 0) {
			this.metadataService.getMetadataElementTypes().subscribe((p) => {
				this.elementTypes = p;
				let selectedType = this.elementTypes.find((_) => _.Name === 'LOV');
				if (selectedType) {
					this.elementType = selectedType.Id;
				}
			});
		} else {
			let selectedType = this.elementTypes.find((p) => p.Name === 'LOV');
			if (selectedType) {
				this.elementType = selectedType.Id;
			}
		}
	}

	removeDerived(result: boolean) {
		this.removeDerivedConfirmationPopupVisible = false;
		if (!result) {
			return;
		}

		this.metadataService
			.removeDerivedElement(this.selectedObject.Id)
			.then(() => this.treeViewService.refreshData(null));
	}

	deactivateElement() {
		this.metadataService
			.deactivateElement(this.selectedObject.Id)
			.then(() => this.treeViewService.refreshData(null));
	}

	selectionChange(element: SchemaElement) {
		this.selectedObject = element;
		if (this.manageTagsVisible) {
			this.hideTagsManagement();
		}
		if (!this.selectedObject || this.selectedObject.Type !== ItemType.Element) {
			return;
		}
		this.allowRemoveDerived$ = this.metadataService
			.getCanDeleteDependentElement(this.selectedObject && this.selectedObject.Id)
			.pipe(map((result) => !result));
	}

	hideDialog() {
		this.newElementName = '';
		this.dialogVisible = false;
	}

	showRemoveDerivedConfirmation() {
		this.removeDerivedConfirmationPopupVisible = true;
	}

	showCreateDerivedElementPopup() {
		this.showDialog(this.selectedObject);
	}

	confirmDeactivation(result: boolean) {
		this.confirmElementDeactivationPopupVisible = false;
		if (!result) {
			return;
		}

		this.deactivateElement();
	}

	showDeactivationConfirmation(itemType: ItemType) {
		this.confirmElementDeactivationPopupVisible = true;
	}

	showTagsManagement() {
		this.manageTagsVisible = true;
		this.showElementDetails = false;
	}

	hideTagsManagement() {
		this.manageTagsVisible = false;
		this.showElementDetails = true;
	}

	downloadElement() {
		this.metadataService.downloadElementDefinition(this.selectedObject.Id, this.selectedObject.Name);
	}
}
