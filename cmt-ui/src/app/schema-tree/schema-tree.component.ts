import { ItemState } from './../shared/item-state';
import { MatInput } from '@angular/material/input';
import { CmtTreeViewComponent } from './../shared/tree-view/tree-view.component';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { TreeViewService, MetadataService, SchemaElement, ItemType } from '../shared';
import { MatSelect } from '@angular/material/select';
import { MatOption } from '@angular/material';

@Component({
	selector: 'cmt-schema-tree',
	templateUrl: 'schema-tree.component.html',
	providers: [ TreeViewService, MetadataService ]
})
export class CmtSchemaTreeComponent implements OnInit {
	public itemType = ItemType;
	data: SchemaElement[];
	notFilteredData: SchemaElement[];
	errorMessage: string;
	loading = false;
	searchTerm: string;
	isFilterActive: boolean;
	selectedChannel = '';
	selectedObject: SchemaElement;
	sourceSchemaId: string;
	popupTitle: string;
	forceReload: boolean;
	@ViewChild(CmtTreeViewComponent) cmtTreeView: CmtTreeViewComponent;
	@ViewChild('channel') channelSelect: MatSelect;
	@ViewChild('schemaName') schemaName: ElementRef<MatInput>;
	addSchemaPopupVisible = false;
	channels: any[] = [ { Name: 'Email', Id: 1 }, { Name: 'Web', Id: 2 } ];
	doNotRefresh: boolean;
	constructor(private treeViewService: TreeViewService, private metadataService: MetadataService) {
		this.refreshData('');
		treeViewService.refreshData$.subscribe((searchTerm) => {
			this.refreshData(searchTerm);
		});
	}

	ngOnInit() {}

	refreshData(searchTerm: string): Promise<any> {
		if (searchTerm != null) {
			this.searchTerm = searchTerm;
		}

		return this.metadataService.getSchemaList(searchTerm).then(
			(schemas) => {
				this.data = schemas as SchemaElement[];
				this.notFilteredData = this.data;
				this.onFilterByType(this.isFilterActive);
				this.errorMessage = null;
				this.forceReload = true;
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
			this.cmtTreeView.filterMessage = 'Show all schemas';
			if (this.data) {
				this.data = this.data.filter((p) => p.Status === true);
			}
		} else {
			this.data = this.notFilteredData;
			this.cmtTreeView.filterMessage = 'Show only active';
		}
		this.selectedObject = null;
	}

	showDialog(source?: any) {
		this.sourceSchemaId = undefined;
		this.metadataService.getChannels().then((c) => {
			this.channels = c as any[];
			if (source) {
				let channel = this.channels.find((ch) => ch.Name === source.Channel);
				this.sourceSchemaId = source.Id;
				if (channel) {
					this.selectedChannel = channel.Id;
				}
			}
		});
		if (source) {
			this.popupTitle = 'Clone schema - ' + source.Name;
		} else {
			this.popupTitle = 'Add Schema';
		}
		this.addSchemaPopupVisible = true;
	}

	selectionChange(element: SchemaElement) {
		if (element.Type === 1) {
			if (!this.selectedObject || this.selectedObject.Id !== element.Id || this.forceReload) {
				this.loading = true;
				this.forceReload = false;
				let existingData = this.data.find(function(item) {
					return item.Id === element.Id;
				});
				if (!existingData.Children || existingData.Children.length === 0) {
					this.metadataService.getSingleSchema(this.searchTerm, element.Id).then(
						(schemas) => {
							this.data = this.data.map(function(item) {
								return item.Id === element.Id ? schemas[0] : item;
							});
							this.errorMessage = null;
							this.selectedObject = schemas[0];
							this.loading = false;
						},
						(error) => {
							this.errorMessage = error;
							this.loading = false;
						}
					);
				} else {
					this.loading = false;
				}
			}
		} else {
			this.selectedObject = element;
			this.loading = false;
		}
	}

	onDialogResult(value: boolean) {
		if (value === true) {
			this.cmtTreeView.isFilterActive = false;
			this.onFilterByType(false);
			let self = this;
			this.metadataService
				.createSchema(
					this.schemaName.nativeElement.value,
					(this.channelSelect.selected as MatOption).value,
					this.sourceSchemaId
				)
				.then((schemaId) => {
					if (schemaId) {
						this.schemaName.nativeElement.value = '';
						this.selectedChannel = null;
						this.addSchemaPopupVisible = false;
						let selectedIndex = this.treeViewService.ItemsState.findIndex((p) => p.isSelected);
						this.treeViewService.ItemsState[selectedIndex].isSelected = false;
						this.treeViewService.ItemsState.push(new ItemState(schemaId, true));
					}
					this.refreshData(this.searchTerm).then(() =>
						setTimeout(function() {
							self.treeViewService.switchToSchemaEditMode();
						}, 1000)
					);
				});
		} else {
			this.schemaName.nativeElement.value = '';
			this.selectedChannel = null;
			// this.channelSelect.writeValue(null);
			this.addSchemaPopupVisible = false;
		}
	}

	get canCreate(): boolean {
		return (
			this.addSchemaPopupVisible &&
			this.schemaName.nativeElement.value &&
			this.channelSelect.selected !== null &&
			this.channelSelect.selected !== undefined
		);
	}
}
