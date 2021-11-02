import {
	Component,
	OnInit,
	Input,
	Output,
	EventEmitter,
	ViewChild,
	AfterViewInit,
	AfterContentInit,
	ContentChildren,
	QueryList,
	DoCheck,
	IterableDiffers,
	KeyValueDiffers
} from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import * as _ from 'lodash';

import { DataTableColumnComponent } from './data-table-column.component';
import { PaginationComponent } from '../pagination/pagination.component';
import { PageChangedEvent } from '../pagination/page-changed-event';
import { LoadDataEvent } from './load-data-event';
import { RowClickEvent } from './row-click-event';
import { SortMetaData } from './sort-meta-data';

@Component({
	selector: 'data-table',
	templateUrl: 'data-table.component.html',
	styleUrls: [ 'data-table.component.css' ],
	viewProviders: [ MatIconRegistry ]
})
export class DataTableComponent implements OnInit, AfterContentInit, DoCheck {
	@Output() loadData: EventEmitter<LoadDataEvent> = new EventEmitter<PageChangedEvent>();
	@Output() selectionChanged: EventEmitter<any[]> = new EventEmitter<any[]>();
	@Output() rowClick: EventEmitter<RowClickEvent> = new EventEmitter<RowClickEvent>();
	@Output() rowDoubleClick: EventEmitter<RowClickEvent> = new EventEmitter<RowClickEvent>();

	@Input() rows: any[];
	@Input() totalRows: number;
	@Input() lazyLoad: boolean;
	@Input() pageable: boolean;
	@Input() pageSize: number;
	@Input() pageSizes: number[];
	@Input() showFirstAndLastPageButtons: boolean;
	@Input() paginationPosition = 'bottom';
	@Input() paginationAlign = 'right';
	@Input() selectionMode: string;
	@Input() selection: any[];
	@Input() sortMode = 'single';
	@Input() sortField: string;
	@Input() sortOrder = 1;
	@Input() multiSortMetaData: SortMetaData[];

	@ContentChildren(DataTableColumnComponent) columns: QueryList<DataTableColumnComponent>;
	@ViewChild(PaginationComponent) private paginationComponent: PaginationComponent;

	isLoading: boolean;
	differ: any;
	startIndex = 0;
	currentPage = 1;
	rowsToRender: Object[];
	itemsPerPageText = 'Rows per page:';
	sortColumn: DataTableColumnComponent;
	protected stopSortPropagation: boolean;

	constructor(differs: IterableDiffers) {
		this.differ = differs.find([]).create(null);
	}

	ngOnInit(): void {
		if (this.lazyLoad) {
			this.isLoading = true;
			this.loadData.emit(this.createLoadDataEvent());
		}
	}

	ngAfterContentInit() {}

	ngDoCheck() {
		let changes = this.differ.diff(this.rows);

		if (changes || (this.rows && this.rows.length == 0)) {
			if (this.pageable) {
				this.totalRows = this.totalRows;
				this.currentPage = this.currentPage;
				this.startIndex = this.startIndex;
			}

			if (this.stopSortPropagation) {
				this.stopSortPropagation = false;
			} else if (!this.lazyLoad && (this.sortField || this.multiSortMetaData)) {
				if (this.sortMode == 'single') {
					this.sortSingle();
				} else if (this.sortMode == 'multiple') {
					this.sortMultiple();
				}
			}

			this.updateRowsToRender(this.rows);
		}
	}

	private onPageChanged(event: PageChangedEvent) {
		this.startIndex = event.startIndex;
		this.pageSize = event.pageSize;
		this.currentPage = event.pageNumber;

		if (this.lazyLoad) {
			this.isLoading = true;
			this.loadData.emit(this.createLoadDataEvent());
		} else {
			this.updateRowsToRender(this.rows);
		}
	}

	private onRowClick(event, row) {
		this.rowClick.emit(new RowClickEvent(event, row));

		if (!this.selectionMode) {
			return;
		}

		if (this.isSelected(row)) {
			if (this.selectionMode === 'single') {
				this.selection = null;
			} else {
				this.selection.splice(this.getSelectedRowIndex(row), 1);
			}
		} else {
			if (this.selectionMode === 'single') {
				this.selection = [ row ];
			} else if (this.selectionMode === 'multiple') {
				this.selection = this.selection || [];
				this.selection.push(row);
			}
		}

		this.selectionChanged.emit(this.selection);
	}

	private onRowDoubleClick(event, row) {
		this.rowDoubleClick.emit(new RowClickEvent(event, row));
	}

	private onSort(event, column: DataTableColumnComponent) {
		if (!column.sortable) {
			return;
		}

		this.sortOrder = this.sortField === column.field ? this.sortOrder * -1 : 1;
		this.sortField = column.field;
		this.sortColumn = column;
		let metaKey = event.metaKey || event.ctrlKey;

		if (this.lazyLoad) {
			this.isLoading = true;
			this.loadData.emit(this.createLoadDataEvent());
		} else {
			if (this.sortMode == 'multiple') {
				if (!this.multiSortMetaData || !metaKey) {
					this.multiSortMetaData = [];
				}

				let index = -1;
				for (let i = 0; i < this.multiSortMetaData.length; i++) {
					if (this.multiSortMetaData[i].field === this.sortField) {
						index = i;
						break;
					}
				}

				if (index >= 0) {
					this.multiSortMetaData[index] = { field: this.sortField, order: this.sortOrder };
				} else {
					this.multiSortMetaData.push({ field: this.sortField, order: this.sortOrder });
				}

				this.sortMultiple();
			} else {
				this.sortSingle();
			}
		}
	}

	private sortSingle() {
		if (this.rows) {
			this.rows.sort((data1, data2) => {
				let value1 = data1[this.sortField];
				let value2 = data2[this.sortField];
				let result = _.lt(value1, value2) ? -1 : _.gt(value1, value2) ? 1 : 0;
				return this.sortOrder * result;
			});

			this.startIndex = 0;
		}

		//prevent resort at ngDoCheck
		this.stopSortPropagation = true;
	}

	private sortMultiple() {
		if (this.rows) {
			this.rows.sort((data1, data2) => {
				return this.multisortField(data1, data2, this.multiSortMetaData, 0);
			});
		}

		//prevent resort at ngDoCheck
		this.stopSortPropagation = true;
	}

	private multisortField(data1, data2, multiSortMeta, index) {
		let value1 = data1[multiSortMeta[index].field];
		let value2 = data2[multiSortMeta[index].field];
		let result = null;

		result = _.lt(value1, value2) ? -1 : 1;

		if (_.eq(value1, value2)) {
			return multiSortMeta.length - 1 > index ? this.multisortField(data1, data2, multiSortMeta, index + 1) : 0;
		}

		return multiSortMeta[index].order * result;
	}

	private updateRowsToRender(rows) {
		if (this.pageable && rows) {
			this.rowsToRender = [];
			let startIndex = this.lazyLoad ? 0 : this.startIndex;

			for (let i = startIndex; i < startIndex + this.pageSize; i++) {
				if (i >= rows.length) {
					break;
				}

				this.rowsToRender.push(rows[i]);
			}
		} else {
			this.rowsToRender = rows;
		}

		if (this.lazyLoad) {
			this.isLoading = false;
		}
	}

	private isSelected(row): boolean {
		return row && this.getSelectedRowIndex(row) != -1;
	}

	private getSelectedRowIndex(row: any): number {
		let index = -1;

		if (this.selection) {
			for (let i = 0; i < this.selection.length; i++) {
				if (_.isEqual(row, this.selection[i])) {
					index = i;
					break;
				}
			}
		}

		return index;
	}

	private getSortOrder(column: DataTableColumnComponent): number {
		let order = 0;
		if (this.sortMode === 'single') {
			if (this.sortField && column.field === this.sortField) {
				order = this.sortOrder;
			}
		} else if (this.sortMode === 'multiple') {
			if (this.multiSortMetaData) {
				for (let i = 0; i < this.multiSortMetaData.length; i++) {
					if (this.multiSortMetaData[i].field == column.field) {
						order = this.multiSortMetaData[i].order;
						break;
					}
				}
			}
		}
		return order;
	}

	private getSortOrderIconName(column: DataTableColumnComponent): string {
		if (this.getSortOrder(column) === -1) {
			return '&#xE5C5;';
		}
		if (this.getSortOrder(column) === 1) {
			return '&#xE5C7;';
		}
		return '';
	}

	private createLoadDataEvent(): LoadDataEvent {
		return {
			count: this.totalRows,
			startIndex: this.startIndex,
			pageSize: this.pageSize,
			sortField: this.sortField,
			sortOrder: this.sortOrder,
			multiSortMetaData: this.multiSortMetaData
		};
	}

	private getVisibleColumnCount(): number {
		return this.columns.filter((o) => o.visible).length;
	}
}
