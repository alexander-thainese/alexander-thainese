import { EventEmitter, Injectable } from '@angular/core';
import { SchemaElement, ItemType } from './schema-element';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ItemState } from './item-state';
import { HttpClient } from './http-client.service';
import { MarketService } from './market/market.service';

@Injectable()
export class TreeViewService {
	selectionSource = new Subject<SchemaElement>();
	editSchema = new Subject<boolean>();
	selection$ = this.selectionSource.asObservable();
	editSchema$ = this.editSchema.asObservable();
	detailsSave$ = new EventEmitter();
	showMore$ = new EventEmitter();
	syncMore$ = new EventEmitter();
	itemsSynched$ = new EventEmitter();
	refreshData$ = new EventEmitter<string>();
	expandElement$ = new EventEmitter();
	expanded$ = new EventEmitter();
	public ItemsState: ItemState[] = [];

	constructor(private http: HttpClient, private readonly marketService: MarketService) {}

	announceNewSelection(selection: SchemaElement) {
		if (selection) {
			let state = this.getItemState(selection);
			if (!state && selection.Id !== 'newid') {
				state = new ItemState();
				state.id = selection.Id;
				state.rootId = selection.RootId;
				state.isSelected = true;
				state.startChildIndex = 0;
				this.ItemsState.push(state);
			}
			if (selection.RootId) {
				this.selectionSource.next(selection);
			}
		}
	}

	announceDetailsSave(): void {
		this.detailsSave$.emit(true);
	}
	announceDetailsCancel(): void {
		this.detailsSave$.emit(false);
	}

	announceShowMore(element: SchemaElement, startIndex: number): void {
		this.showMore$.emit({ element: element, startIndex: startIndex });
	}

	announceExpanded(): void {
		this.expanded$.emit();
	}

	// getMoreItems(parentId: string, type: ItemType): Promise<SchemaElement[]> {
	//     return this.http.get(this.getMoreElementsUrl)
	//         .toPromise()
	//         .then(response => response as SchemaElement[])
	//         .catch(this.handleError);
	// }

	syncToItem(selectedIndex: number): void {
		this.syncMore$.emit(selectedIndex);
	}

	itemsSynched(): void {
		this.itemsSynched$.emit();
	}

	previewData(data: Object): void {}

	refreshData(searchTerm: string) {
		this.refreshData$.emit(searchTerm);
	}

	expand(id: string) {
		this.expandElement$.emit(id);
	}

	getDownloadSchemaUrl(schemaId: string): string {
		return environment.baseUrl + 'api/metadata/' + schemaId;
	}

	getDownloadElementUrl(elementId: string) {
		return (
			environment.baseUrl +
			'api/AdminMetadata/GetElementDefinition/' +
			elementId +
			'/' +
			this.marketService.marketCode
		);
	}

	handleError(): void {
		alert('Error occured');
	}

	getItemState(element: SchemaElement): ItemState {
		if (element) {
			return this.ItemsState.find((p) => p.id === element.Id && p.rootId === element.RootId);
		} else {
			return new ItemState();
		}
	}

	switchToSchemaEditMode() {
		this.editSchema.next();
	}
}
