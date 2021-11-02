import { Component, OnInit, Input, OnChanges, OnDestroy, SimpleChanges, Pipe, PipeTransform } from '@angular/core';
import { SchemaElement, TreeViewService, ItemState, ItemType, MetadataService } from '../../index';
import { Subscription } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '../../http-client.service';

@Component({
	selector: 'cmt-treelist-element',
	templateUrl: 'treelist-element.component.html'
})
export class CmtTreelistElementComponent implements OnInit, OnDestroy, OnChanges {
	private isExpanding: boolean;
	_element: SchemaElement;
	@Input() rootId: string;
	@Input() depth = 1;
	@Input() isLast: boolean;
	@Input() lazyLoad: boolean;
	isLastItem = false;
	isLoaded = false;
	isSelected = false;
	isExpanded = false;
	subscription: Subscription;
	itemType = ItemType;

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient
	) {
		this.isExpanding = false;
		this.subscription = treeViewService.selection$.subscribe((schemaElement) => {
			this.checkSelection(schemaElement);
		});
		treeViewService.expandElement$.subscribe((id) => {
			if (id === this.element.Id) {
				this.isExpanded = true;
				this.updateState();
				treeViewService.announceExpanded();
			}
		});
	}

	public get element() {
		return this._element;
	}

	@Input()
	public set element(element: SchemaElement) {
		this._element = element;
		this.treeViewService.itemsSynched();
		if (this._element && this._element.Id === 'newid') {
			this.isSelected = true;
			// this.updateState();
		}
		let result = this.getItemState();
		if (result) {
			this.isExpanded = result.isExpanded;
			this.isSelected = result.isSelected;
			this.element.RootId = this.rootId;
			if (this.isSelected) {
				setTimeout(() => {
					this.treeViewService.announceNewSelection(this.element);
				}, 500);
			}
		}
	}

	ngOnInit() {
		this.treeViewService.syncMore$.subscribe((childIndex) => {
			if (this.isSelected) {
				this.updateState();
			}
		});
	}

	public get limitedItems(): SchemaElement[] {
		if (!this.element || !this.element.Children) {
			return null;
		} else {
			return this.element.Children.filter(
				(item, index) =>
					item.Type === ItemType.Element ||
					(item.Children && item.Children.length > 0 && item.Status === true && item.SearchTermFound)
			);
		}
	}

	checkSelection(schemaElement: SchemaElement) {
		if (!this.element) {
			return;
		}
		if (this.element && schemaElement.Id === this.element.Id && this.rootId === schemaElement.RootId) {
			if (!this.element.Name) {
				this.treeViewService.announceNewSelection(this.element);
			} else {
				this.isSelected = true;
			}
		} else {
			this.isSelected = false;
		}

		this.updateState();
	}

	select(): void {
		this.selectInternal(false);
	}

	toggleExpand(): void {
		this.isExpanded = !this.isExpanded;
	}

	ngOnDestroy() {
		this.subscription.unsubscribe();
	}

	ngOnChanges(changes: SimpleChanges) {}

	getItemState(): ItemState {
		if (this.element) {
			if (this.treeViewService.ItemsState.length === 0) {
				let state = new ItemState();
				state.id = this.element.Id;
				state.rootId = this.element.Id;
				state.isSelected = true;
				state.isExpanded = false;
				this.treeViewService.ItemsState.push(state);
			}
			return this.treeViewService.ItemsState.find((p) => p.id === this.element.Id && p.rootId === this.rootId);
		} else {
			return new ItemState();
		}
	}

	delete(id: string, type: ItemType) {
		this.metadataService.deleteElement(id, type).then(() => {
			this.treeViewService.refreshData('');
		});
	}

	get allowDelete(): boolean {
		return this.http.isDev;
	}

	private updateState() {
		if (this.element.Id === 'newid') {
			return;
		}
		let state = this.getItemState();
		let index = this.treeViewService.ItemsState.indexOf(state);
		if (state && index > -1) {
		} else {
			state = new ItemState();
			this.treeViewService.ItemsState.push(state);
		}
		state.id = this.element.Id;
		state.rootId = this.rootId;
		state.isExpanded = this.isExpanded;
		state.isSelected = this.isSelected;
	}

	private selectInternal(expand: boolean): void {
		this.element.RootId = this.rootId;
		if (expand && ((this.isExpanded && this.isSelected) || !this.isExpanded)) {
			this.toggleExpand();
		}
		this.isSelected = true;
		this.treeViewService.announceNewSelection(this.element);
	}

	get showExpansion(): boolean {
		if (this.element.Id === this.rootId && this.lazyLoad) {
			return this.element.HasChildren || (this.limitedItems && this.limitedItems.length > 0);
		}
		return this.limitedItems && this.limitedItems.length > 0;
	}
}
