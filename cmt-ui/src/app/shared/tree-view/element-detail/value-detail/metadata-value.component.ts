import { of as observableOf, Observable, Subject, Subscription } from 'rxjs';
import { PageChangedEvent } from './../../../pagination/page-changed-event';
import { Component, OnInit, Input, Output, SimpleChanges, OnChanges, EventEmitter, OnDestroy } from '@angular/core';
import { OnSave } from '../on-save.interface';
import { SchemaElement, MetadataService, TreeViewService, ItemType } from '../../../index';
import { CmtInlineEditComponent } from '../../../inline-edit';
import { CmtInlineSearch } from '../../../inline-search';
import { environment } from '../../../../../environments/environment';
import { HttpClient } from '../../../http-client.service';
import { MarketService } from '../../../market/market.service';
import { takeUntil, take } from 'rxjs/operators';
import { ParentValueAttribute } from '../../../parent-value-attribute';
import { ValueTag } from '../../../../metadata-elements/manage-element-tags/manage-element-tags.component';

@Component({
	selector: 'cmt-metadata-value-detail',
	templateUrl: 'metadata-value.component.html'
})
export class CmtMetadataValueDetailComponent implements OnInit, OnChanges, OnDestroy {
	destroyed$ = new Subject<boolean>();
	valueTags: ValueTag[] = [];
	parentValueAttributes: ParentValueAttribute[] = [];
	get sortedChildren(): SchemaElement[] {
		return this.selectedObject.Children.sort(this.sortChildren);
	}

	get allowEdit(): boolean {
		return this.http.isAdmin;
	}

	get allowDeactivate(): boolean {
		return this.http.isAdmin;
	}

	get childrenCount(): number {
		return this.selectedObject.Children ? this.selectedObject.Children.length : 0;
	}

	get localValuesCount(): number {
		return this.selectedObject.Children ? this.selectedObject.Children.filter((p) => p.LocalValue).length : 0;
	}
	@Input() selectedObject: SchemaElement;
	@Input() readonly = true;
	@Output() saved = new EventEmitter<boolean>();
	@Input() availableCountries: string[];
	@Input() reverseColumns = false;
	@Output() addChild = new EventEmitter<SchemaElement>();
	valueExpanded: boolean[] = [];
	country: string;
	countryCode: string;
	newName: string;
	newLocalValue: string;
	itemType = ItemType;
	localSearchValue: string;
	globalSearchValue: string;
	localCodeSearchValue: string;
	globalCodeSearchValue: string;
	expandSubscription: Subscription;
	_filteredValues: SchemaElement[];
	filteredValues: Observable<SchemaElement[]>;

	// paging
	pageSize = 10;
	pageSizes: number[] = [ 10, 20, 50, 100 ];
	totalRows = 0;
	currentPage = 0;
	startIndex = 0;

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient,
		private readonly marketService: MarketService
	) {}

	ngOnInit() {
		this.marketService.marketChanged.pipe(takeUntil(this.destroyed$)).subscribe((p) => {
			this.country = p.Name;
		});
	}
	ngOnDestroy(): void {
		this.destroyed$.next(true);
		this.destroyed$.complete();
	}

	isRowExpandable(el: SchemaElement) {
		return (
			el.HasTags === true ||
			(this.selectedObject &&
				this.selectedObject.ParentValuesAttributes &&
				this.selectedObject.ParentValuesAttributes.some((p) => p.ParentId === el.ParentValueId))
		);
	}

	filterValues(): Observable<SchemaElement[]> {
		this._filteredValues = this.selectedObject.Children;
		if (this.localSearchValue) {
			this._filteredValues = this._filteredValues.filter(
				(p) => p.LocalValue && p.LocalValue.toLowerCase().indexOf(this.localSearchValue.toLowerCase()) > -1
			);
		}
		if (this.globalSearchValue) {
			this._filteredValues = this._filteredValues.filter(
				(p) => p.Name.toLowerCase().indexOf(this.globalSearchValue.toLowerCase()) > -1
			);
		}
		if (this.globalCodeSearchValue) {
			this._filteredValues = this._filteredValues.filter(
				(p) => p.GlobalCode && p.GlobalCode.toLowerCase().indexOf(this.globalCodeSearchValue.toLowerCase()) > -1
			);
		}
		if (this.localCodeSearchValue) {
			this._filteredValues = this._filteredValues.filter(
				(p) => p.LocalCode && p.LocalCode.toLowerCase().indexOf(this.localCodeSearchValue.toLowerCase()) > -1
			);
		}
		let i = 0;
		this.totalRows = this._filteredValues.length;
		return observableOf(this._filteredValues.slice(this.startIndex, this.pageSize));
	}

	public onCancel(): void {
		this.newName = this.selectedObject.Name.toString();
		if (this.selectedObject.LocalValue) {
			this.newLocalValue = this.selectedObject.LocalValue.toString();
		} else {
			this.newLocalValue = '';
		}
	}

	public onUpdateName(id: any, name: any, valueEditor): void {
		this.metadataService.updateValueTextValue(id, name).then(
			(result) => {
				if (result === '') {
					valueEditor.restoreValue();
				} else {
					this.onSaved(result);
				}
			},
			(error) => {
				valueEditor.restoreValue();
				this.saved.emit(false);
			}
		);
	}

	public onUpdateCode(id: any, name: any, code: any, valueEditor): void {
		this.metadataService.updateValueGlobalCode(id, name, code).then(
			(result) => {
				if (result === '') {
					valueEditor.restoreValue();
				} else {
					this.onSaved(result);
				}
			},
			(error) => {
				valueEditor.restoreValue();
				this.saved.emit(false);
			}
		);
	}

	public onUpdateTranslationCode(id: any, value: any, code: any, translationEditor): void {
		this.metadataService.TranslateValue({ Id: id, LocalCode: code, LocalValue: value }).then(
			(result) => {
				if (result === '') {
					translationEditor.restoreValue();
				} else {
					this.onSaved(result);
				}
			},
			(error) => {
				translationEditor.restoreValue();
				this.saved.emit(false);
				alert('Saving translation failed');
			}
		);
	}

	public onUpdateTranslation(id: any, value: any, translationEditor): void {
		this.metadataService.TranslateValue({ Id: id, LocalValue: value }).then(
			(result) => {
				if (result === '') {
					translationEditor.restoreValue();
				} else {
					this.onSaved(result);
				}
			},
			(error) => {
				translationEditor.restoreValue();
				this.saved.emit(false);
				alert('Saving translation failed');
			}
		);
	}

	public addChildClick(source: SchemaElement) {
		this.addChild.emit(source);
	}

	public onSave(): void {}
	ngOnChanges(changes: SimpleChanges) {
		let c = changes as any;
		if (c.selectedObject && c.selectedObject.currentValue) {
			this.localSearchValue = '';
			this.globalSearchValue = '';
			this.localCodeSearchValue = '';
			this.globalCodeSearchValue = '';
			this.filteredValues = observableOf([]);
			this.newName = c.selectedObject.currentValue.Name.toString();
			if (c.selectedObject.currentValue.LocalValue) {
				this.newLocalValue = c.selectedObject.currentValue.LocalValue.toString();
			} else {
				this.newLocalValue = '';
			}
			this.selectedObject.Children = this.sortedChildren;
			this.selectedObject.Children.forEach((item) => {
				item.Readonly = this.calculateReadonly(item);
			});
			this.onSearch('');
		}
	}

	sortChildren(n1: SchemaElement, n2: SchemaElement) {
		if (n1.Status < n2.Status) {
			return 1;
		}
		if (n1.Status > n2.Status) {
			return -1;
		} else {
			if (n1.Name.toLowerCase() > n2.Name.toLowerCase()) {
				return 1;
			}
			if (n1.Name.toLowerCase() < n2.Name.toLowerCase()) {
				return -1;
			}

			return 0;
		}
	}

	deactivate(id: string, type: ItemType) {
		this.metadataService.deleteElement(id, type).then(() => {
			this.treeViewService.refreshData('');
		});
	}

	activate(id: string, type: ItemType) {
		this.metadataService.activateElement(id, type).then(() => {
			this.treeViewService.refreshData('');
		});
	}

	expandChild(element: SchemaElement) {
		if (element.Status === false) {
			return;
		}
		element.RootId = this.selectedObject.RootId;
		this.expandSubscription = this.treeViewService.expanded$.subscribe(() => {
			setTimeout(() => this.treeViewService.announceNewSelection(element), 700);
			this.expandSubscription.unsubscribe();
		});
		this.treeViewService.expand(element.ParentId);
	}
	track(index, item) {
		if (item) {
			return item.Id;
		}
		return undefined;
	}

	removeValueTag(id: string, valueId: string) {
		this.metadataService.removeValueTag(id).pipe(take(1)).subscribe((p) => {
			this.treeViewService.refreshData('');
			this.metadataService.getValueTags(valueId).pipe(take(1)).subscribe((v) => (this.valueTags[valueId] = v));
		});
	}

	onSearch(query) {
		let that = this;
		this.filteredValues = observableOf([]);
		this.currentPage = 1;
		this.startIndex = 0;
		setTimeout(function() {
			that.filteredValues = that.filterValues();
		}, 650);
	}

	onPageChanged(data: PageChangedEvent) {
		this.currentPage = data.pageNumber;
		this.startIndex = data.startIndex;
		this.pageSize = data.pageSize;
		this.filteredValues = observableOf(
			this._filteredValues.slice(this.startIndex, this.currentPage * this.pageSize)
		);
	}

	toggleValueDetails(id: string, parentValueId: string, hasTags: boolean) {
		this.valueExpanded[id] = !this.valueExpanded[id];
		if (this.valueExpanded[id] && hasTags) {
			this.metadataService.getValueTags(id).pipe(take(1)).subscribe((p) => (this.valueTags[id] = p));
		} else {
			this.valueTags[id] = undefined;
		}
		if (parentValueId !== undefined && parentValueId !== null) {
			this.parentValueAttributes[id] = this.selectedObject.ParentValuesAttributes.filter(
				(p) => p.ParentId === parentValueId
			);
		}
	}

	private onSaved(result: string | void, refreshSelection?: boolean) {
		if (refreshSelection) {
			this.treeViewService.announceNewSelection(this.selectedObject);
		}
		this.saved.emit(true);
		this.selectedObject.Children = this.sortedChildren;
	}

	private isValid(): boolean {
		if (!this.newName || this.newName === null || this.newName === '') {
			return false;
		}

		return true;
	}
	private calculateReadonly(element: SchemaElement) {
		return this.readonly || !this.allowEdit || element.Readonly || element.Status === false;
	}
}
