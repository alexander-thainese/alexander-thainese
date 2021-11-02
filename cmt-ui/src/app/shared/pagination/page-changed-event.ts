import { EventBase } from '../events';
import { PaginationComponent } from './pagination.component';

export class PageChangedEvent extends EventBase<PaginationComponent> {
	count: number;
	pageNumber: number;
	startIndex: number;
	pageSize: number;

	constructor(source: PaginationComponent, pageNumber: number, startIndex: number, count: number) {
		super(source);
		this.startIndex = startIndex;
		this.pageSize = count;
		this.pageNumber = pageNumber;
	}
}
