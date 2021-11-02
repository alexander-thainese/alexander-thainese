import {
	Component,
	OnInit,
	OnChanges,
	SimpleChanges,
	ElementRef,
	Input,
	Output,
	EventEmitter,
	ViewChild
} from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { MatIconRegistry } from '@angular/material/icon';
import {
	CmtMetadataSchemaDetailComponent,
	CmtMetadataElementDetailComponent,
	CmtMetadataValueDetailComponent
} from './details';
import { Subject, Subscription } from 'rxjs';
import { SchemaElement, ItemType, MetadataService, TreeViewService } from '../../index';
import { DialogComponent } from '../../dialog';
import { HttpClient } from '../../http-client.service';
import { checkFlag } from '../../common.functions';

// declare var jQuery: any;

@Component({
	selector: 'cmt-element-detail',
	templateUrl: 'element-detail.component.html',
	viewProviders: [ MatIconRegistry ]
})
export class CmtElementDetailComponent implements OnInit, OnChanges {
	get addNewValueFor() {
		return 'Add new child value to ' + this.valueForName;
	}

	get allowSwitchView(): boolean {
		return this.selectedObject.Type === ItemType.Element && this.selectedObject.IsLov;
	}

	get showSchemaMenu(): boolean {
		return (
			this.selectedObject.Type === ItemType.Schema &&
			(this.allowDelete || this.allowEdit || this.allowActivate || this.allowDeactivate)
		);
	}

	get allowEdit(): boolean {
		return this.selectedObject && this.http.isAdmin;
	}

	get allowDelete(): boolean {
		return (
			this.http.isAdmin &&
			this.selectedObject &&
			!this.selectedObject.Status &&
			!this.selectedObject.ActivationDate
		);
	}

	get allowActivate(): boolean {
		return this.http.isAdmin && this.selectedObject && !this.selectedObject.Status;
	}

	get allowDeactivate(): boolean {
		return this.http.isAdmin && this.selectedObject && this.selectedObject.Status;
	}

	get allowClone(): boolean {
		return this.http.isAdmin;
	}

	get allowAddChild(): boolean {
		return (
			this.http.isAdmin &&
			!this.readonly &&
			this.itemSelected &&
			!this.selectedObject.Readonly &&
			(this.selectedObject.Level < 3 || !this.selectedObject.Level) &&
			this.selectedObject.Type !== ItemType.Schema &&
			(this.selectedObject.Type !== ItemType.Element || this.selectedObject.IsLov)
		);
		// tslint:disable-next-line: max-line-length
		// return this.editable && this.itemSelected && this.selectedObject.Level < 3 && (this.selectedObject.Type == ItemType.Item || (this.selectedObject.Type == ItemType.Element && this.selectedObject.IsLov)
		//     || this.selectedObject.Type == ItemType.Group);
	}

	get displayName(): string {
		if (this.selectedObject.Type === ItemType.Schema) {
			return this.selectedObject.Name;
		} else if (this.selectedObject.Type === ItemType.Group) {
			return this.selectedObject.Name;
		} else if (this.selectedObject.Type === ItemType.Element) {
			return 'Element details - ' + this.selectedObject.Name;
		} else {
			if (this.selectedObject.Children && this.selectedObject.Children.length > 0) {
				return 'LOVs (' + this.selectedObject.Children[0].LevelName + ') - ' + this.selectedObject.Name;
			} else {
				return 'LOVs - ' + this.selectedObject.Name;
			}
		}
	}

	get showLovDetails() {
		return this.selectedObject.Type === ItemType.Item || (this.showLov && this.allowSwitchView);
	}
	// @ViewChild(CmtMetadataElementDetailComponent) private elementDetail: CmtMetadataElementDetailComponent;
	// @ViewChild(CmtMetadataSchemaDetailComponent) private schemaDetail: CmtMetadataSchemaDetailComponent;
	// @ViewChild(CmtMetadataValueDetailComponent) private valueDetail: CmtMetadataValueDetailComponent;
	@ViewChild(DialogComponent) dialog: DialogComponent;

	@Input() selectedObject: SchemaElement;
	@Input() hideValueDetails = false;
	@Input() readonly = false;
	@Output() cloneSchema = new EventEmitter();
	@Output() addElement = new EventEmitter();
	@Input() showContent = false;
	schemaEdit = false;
	public itemType = ItemType;
	public itemSelected = false;
	saving = false;
	// items: SchemaElement[];
	showLov = true;
	rootId: string;
	parent: SchemaElement;
	totatRows: number;
	elementRef: ElementRef;
	dialogVisible: boolean;
	valueForName: string;
	valurForId: string;
	expandSubscription: Subscription;
	confirmActivationPopupVisible = false;
	// confirmElementDeactivationPopupVisible = false;
	confirmSchemaDeactivationPopupVisible = false;
	checkFlag = checkFlag;

	constructor(
		private route: ActivatedRoute,
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		elementRef: ElementRef,
		private http: HttpClient
	) {
		this.elementRef = elementRef;
	}

	onAddChild(source: SchemaElement) {
		this.showDialog(source.Id, source.Name);
	}

	addValue(value) {
		if (value) {
			value = value.trim();
			let element = new SchemaElement();
			element.ParentId = this.valurForId;
			element.Name = value;
			this.metadataService.addChildValue(element).then((result) => {
				if (!result) {
					return;
				}
				this.expandSubscription = this.treeViewService.expanded$.subscribe(() => {
					let sel = new SchemaElement();
					sel.Id = element.ParentId;
					sel.RootId = this.selectedObject.RootId;
					this.treeViewService.announceNewSelection(sel);
					setTimeout(() => this.treeViewService.refreshData(''), 700);
					this.expandSubscription.unsubscribe();
				});
				if (element.ParentId) {
					this.treeViewService.expand(element.ParentId);
					this.treeViewService.expand(this.selectedObject.Id);
				} else {
					this.treeViewService.expand(this.selectedObject.Id);
				}
			});
		}
		this.dialog.value = '';
		this.hideDialog();
	}

	showDialog(id: string, name: string) {
		this.valueForName = name;
		this.valurForId = id;
		this.dialogVisible = true;
	}

	hideDialog() {
		this.dialogVisible = false;
	}

	ngOnInit() {
		let id = +this.route.snapshot.params['id'];
		this.treeViewService.selection$.subscribe((selection) => {
			this.selectionChanged(selection);
		});
		this.treeViewService.editSchema$.subscribe(() => (this.schemaEdit = true));
	}

	editClick() {
		this.schemaEdit = true;
	}

	addChild() {
		let element = new SchemaElement();
		element.Name = 'New Item';
		if (this.selectedObject.Type === ItemType.Group) {
			element.Type = ItemType.Element;
			element.IsLov = true;
		} else {
			element.Type = ItemType.Item;
		}
		element.Id = 'newid';
		element.Children = null;
		element.RootId = this.selectedObject.RootId;
		element.ParentId = this.selectedObject.Id;
		this.treeViewService.expand(element.ParentId);
		this.parent = this.selectedObject;
		if (!this.selectedObject.Children) {
			this.selectedObject.Children = [ element ];
		} else {
			if (this.selectedObject.Children.length > 3) {
				this.treeViewService.syncToItem(this.selectedObject.Children.length - 3);
			}
			this.selectedObject.Children.push(element);
		}
		this.selectedObject = element;
		this.treeViewService.announceNewSelection(element);
	}

	saveClick() {
		this.saving = true;
		this.treeViewService.announceDetailsSave();
	}
	onSaved(result: string) {}

	downloadSchema(): void {
		window.open(this.treeViewService.getDownloadSchemaUrl(this.selectedObject.Id));
	}

	switchViewClick(): void {
		this.showLov = !this.showLov;
	}

	selectionChanged(selection: SchemaElement) {
		this.itemSelected = true;
	}

	confirmActivation(result: boolean) {
		if (result) {
			this.activateSchema();
		}
		this.confirmActivationPopupVisible = false;
	}

	confirmDeactivation(result: boolean) {
		if (!result) {
			this.confirmSchemaDeactivationPopupVisible = false;
			return;
		}

		switch (this.selectedObject.Type) {
			case ItemType.Schema:
				this.deactivateSchema();
				break;
			default:
				alert('this type of object cannot be deactivated');
		}
		this.confirmSchemaDeactivationPopupVisible = false;
	}

	activateSchema() {
		this.metadataService.activateSchema(this.selectedObject.Id).then(() => this.treeViewService.refreshData(null));
	}

	deactivateSchema() {
		this.metadataService
			.deactivateSchema(this.selectedObject.Id)
			.then(() => this.treeViewService.refreshData(null));
	}

	deleteSchema() {
		this.metadataService.deleteSchema(this.selectedObject.Id).then(() => this.treeViewService.refreshData(null));
	}

	onCloneSchema() {
		this.cloneSchema.emit({
			Id: this.selectedObject.Id,
			Name: this.selectedObject.Name,
			Channel: this.selectedObject.Channel
		});
	}

	onCloseSchemaEdit(result) {
		this.schemaEdit = false;
		this.treeViewService.refreshData(null);
	}

	showActivationConfirmation() {
		this.confirmActivationPopupVisible = true;
	}

	showDeactivationConfirmation(itemType: ItemType) {
		this.confirmSchemaDeactivationPopupVisible = true;
	}

	ngOnChanges(changes: SimpleChanges) {
		if (changes['selectedObject']) {
			let obj = changes['selectedObject'];
			if (obj.previousValue && obj.currentValue && obj.currentValue.Id !== obj.previousValue.Id) {
				this.schemaEdit = false;
			}
		}
	}

	updateElementName(id: string, name: string, valueEditor) {
		this.metadataService.updateElementName(id, name).then(
			(result) => {
				if (result === '') {
					valueEditor.restoreValue();
				} else {
					this.onSaved(result);
				}
			},
			(error) => {
				valueEditor.restoreValue();
				// this.onSaved(false);
			}
		);
	}
}
