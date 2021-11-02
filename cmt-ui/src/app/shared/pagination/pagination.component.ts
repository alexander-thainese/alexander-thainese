import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';

import { EventBase } from '../events';
import { PageChangedEvent } from './page-changed-event';

@Component({
	selector: 'pagination',
	templateUrl: 'pagination.component.html',
	styleUrls: [ 'pagination.component.css' ],
	viewProviders: [ MatIconRegistry ]
})
export class PaginationComponent {
	@Output() pageChanged: EventEmitter<PageChangedEvent> = new EventEmitter<PageChangedEvent>();
	@Input() startIndex: number;
	@Input() currentPage: number;
	@Input() pageSize: number;
	@Input() pageSizes: number[];
	@Input() itemsPerPageText = 'Items per page:';
	@Input() showFirstAndLastPageButtons: boolean;
	@Input() totalItems: number;
	@Input() showPageSizeSelector = true;

	constructor() {}

	isFirstPage(): boolean {
		return this.currentPage === 1;
	}

	isLastPage(): boolean {
		return this.currentPage >= this.getLastPage();
	}

	nextPage(): void {
		if (!this.isLastPage()) {
			this.changePage(1);
		} else {
			return;
		}
	}

	prevoiusPage(): void {
		if (!this.isFirstPage()) {
			this.changePage(-1);
		} else {
			return;
		}
	}

	firstPage(): void {
		if (!this.isFirstPage()) {
			this.setPage(1);
		} else {
			return;
		}
	}

	lastPage(): void {
		if (!this.isLastPage()) {
			this.setPage(this.getLastPage());
		} else {
			return;
		}
	}

	changePage(offset: number): void {
		this.setPage(this.currentPage + offset);
	}

	setPage(pageNumber: number): void {
		this.currentPage = pageNumber;
		this.startIndex = (this.currentPage - 1) * this.pageSize;
		const event = new PageChangedEvent(this, this.currentPage, this.startIndex, this.pageSize);
		this.pageChanged.emit(event);
	}

	getLastPage(): number {
		if (this.totalItems % this.pageSize > 0) {
			return Math.floor(this.totalItems / this.pageSize) + 1;
		} else {
			return this.totalItems / this.pageSize;
		}
	}

	onPageSizeChanged(pageSize: number) {
		this.setPage(1);
	}
}
