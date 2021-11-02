import { ListValue } from './../../../schema-element';
import { Component, OnInit, OnChanges, Input, Output, SimpleChanges, OnDestroy, EventEmitter } from '@angular/core';
import { AbstractControl, FormControl, Validators } from '@angular/forms';
import { OnSave } from '../on-save.interface';
import { TreeViewService, MetadataService, SchemaElement, ItemType } from '../../../index';
import { Subscription, Subject } from 'rxjs';
import { MetadataElement } from './metadata-element';
import { ExternalSystem } from './external-system';
import { CmtInlineEditComponent } from '../../../inline-edit';
import { HttpClient } from '../../../http-client.service';
import { takeUntil } from 'rxjs/operators';

@Component({
	selector: 'cmt-metadata-element-detail',
	templateUrl: 'metadata-element.component.html'
})
export class CmtMetadataElementDetailComponent implements OnInit, OnSave, OnChanges, OnDestroy {
	destroyed$ = new Subject<boolean>();
	get allowEdit(): boolean {
		return this.http.isAdmin;
	}

	get sortedSystems(): ExternalSystem[] {
		if (this.systems) {
			return this.systems.sort((n1, n2) => {
				if (n1.Name > n2.Name) {
					return 1;
				}

				if (n1.Name < n2.Name) {
					return -1;
				}

				return 0;
			});
		} else {
			return this.systems;
		}
	}
	@Input() editMode = false;
	@Input() readonly = true;
	@Input() selectedObject: SchemaElement;
	@Output() saved = new EventEmitter<boolean>();
	itemType: ItemType;
	defaultDisplayText: string;

	sourceMdElement: MetadataElement;
	mdElement: MetadataElement;
	subscription: Subscription;
	systems: ExternalSystem[];

	dependentElements = [];
	parentElement: ListValue;

	defaultValue: ListValue;
	dropdownItems: ListValue[] = [];

	showChangeTypePopupVisible = false;

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient
	) {
		this.mdElement = new MetadataElement();
	}

	ngOnInit() {}

	public onCancel(): void {
		this.resetToDefault();
	}

	public onSave(): void {
		if (this.sourceMdElement.LovLabels && this.mdElement.LovLabels) {
			this.sourceMdElement.LovLabels = this.mdElement.LovLabels;
		}
		this.sourceMdElement.Description = this.mdElement.Description.toString();
		this.sourceMdElement.Name = this.mdElement.Name.toString();

		if (this.selectedObject.Id === 'newid') {
			this.sourceMdElement.GroupId = this.selectedObject.RootId;
			this.metadataService.addElement(this.sourceMdElement).then(
				(result) => {
					this.selectedObject.Id = result;
					this.sourceMdElement.Id = result;
					this.mdElement.Id = result;
					this.onSaved('ok', true);
				},
				(error) => {
					console.log('error adding new value'), this.saved.emit(false);
				}
			);
		} else {
			this.metadataService.updateMetadataElementDetails(this.sourceMdElement).then(
				(result) => {
					this.saved.emit(true);
				},
				(error) => {
					this.saved.emit(false);
					alert('Saving value failed');
				}
			);
		}
	}

	public showChangeTypePopup() {
		if (!this.readonly) {
			this.showChangeTypePopupVisible = true;
		}
	}

	updateLovLabels() {
		this.metadataService.updateMetadataElementDetails(this.sourceMdElement).then(
			(result) => {
				this.treeViewService.refreshData('');
			},
			(error) => {
				this.saved.emit(false);
				alert('Saving value failed');
			}
		);
	}

	public updateElementDescription(id: string, description: string) {
		this.metadataService.updateMetadataElementDescription(id, description).then(
			(result) => {
				this.metadataService.getMetadataElementDetails(this.selectedObject.Id).then((mde) => {
					this.sourceMdElement = mde as MetadataElement;
				});
			},
			(error) => {
				this.saved.emit(false);
				alert('Saving value failed');
			}
		);
	}

	onSaved(result: string, refreshSelection?: boolean) {
		if (refreshSelection) {
			this.treeViewService.announceNewSelection(this.selectedObject);
		}
		this.saved.emit(true);
	}

	ngOnChanges(changes: SimpleChanges) {
		let c = changes as any;
		if (c.selectedObject) {
			if (this.selectedObject.Id === 'newid') {
				this.sourceMdElement = new MetadataElement();
				this.sourceMdElement.Name = this.selectedObject.Name;
				this.defaultValue = new ListValue();
				this.dropdownItems = [];
			} else {
				this.dropdownItems = [];
				this.dropdownItems = this.selectedObject.Children
					.filter((p) => p.Status)
					.map((p) => new ListValue(p.Name, p.Id));
				this.defaultValue = this.selectedObject.DefaultValue
					? this.selectedObject.DefaultValue
					: new ListValue();
				this.defaultDisplayText =
					this.selectedObject.DefaultValue && this.selectedObject.DefaultValue.Name
						? this.selectedObject.DefaultValue.Name
						: 'No default';
				if (!this.defaultValue.Name) {
					this.defaultValue.Name = 'No default';
				}
				this.dependentElements = [];
				this.parentElement = undefined;

				this.metadataService.getMetadataElementDetails(this.selectedObject.Id).then((mde) => {
					this.sourceMdElement = mde as MetadataElement;
				});
				this.metadataService.getThirdPartySystemsAccess(this.selectedObject.Id, false).then((res) => {
					this.systems = res as ExternalSystem[];
				});
				// tslint:disable-next-line: no-bitwise
				if ((this.selectedObject.Attributes & 4) === 4) {
					this.metadataService.getParentElement(this.selectedObject.Id).then((res) => {
						this.parentElement = res;
					});
				} else {
					this.metadataService.getDependentElements(this.selectedObject.Id).then((res) => {
						this.dependentElements = res;
					});
				}
			}
		}
		if (c.editMode && this.editMode === true) {
			this.resetToDefault();
		}
	}

	ngOnDestroy() {
		this.destroyed$.next(true);
		this.destroyed$.complete();
		// this.subscription.unsubscribe();
	}

	track(index, item) {
		return false;
	}

	selectionChanged(system: ExternalSystem, data: any) {
		this.metadataService
			.updateThirdPartySystemsAccess(system, false)
			.catch((error) => (data.source.checked = !data.source.checked));
	}

	selectElement(event, id: string) {
		event.preventDefault();
		let element = new SchemaElement();
		element.Id = id;
		element.RootId = id;
		this.treeViewService.announceNewSelection(element);
	}

	setDefaultValue(value: string) {
		if (value && this.selectedObject.IsLov) {
			this.selectedObject.DefaultValue.Name = this.dropdownItems.find((p) => p.Value === value).Name;
		}
		this.metadataService.setDefaultValue(this.selectedObject.Id, value);
	}

	public changeElementType(typeId: string): void {
		this.showChangeTypePopupVisible = false;
		if (typeId && typeId !== this.sourceMdElement.TypeId) {
			this.metadataService
				.updateElementType(this.selectedObject.Id, typeId)
				.pipe(takeUntil(this.destroyed$))
				.subscribe((_) => this.treeViewService.refreshData(''));
		}
	}

	private resetToDefault(): void {
		if (this.sourceMdElement.Description) {
			this.mdElement.Description = this.sourceMdElement.Description.toString();
		} else {
			this.mdElement.Description = '';
		}
		this.mdElement.Name = this.sourceMdElement.Name.toString();
		if (this.sourceMdElement.LovLabels) {
			this.mdElement.LovLabels = this.sourceMdElement.LovLabels.slice(0);
		}
	}

	private getDropDownValues() {}

	private getDependentElements() {
		this.dependentElements = [ { Name: 'Dependent 1', Value: 1 }, { Name: 'Dependent 2', Value: 2 } ];
	}

	private isValid(): boolean {
		if (!this.mdElement.Name || this.mdElement.Name == null || this.mdElement.Name === '') {
			return false;
		}

		return true;
	}
}
