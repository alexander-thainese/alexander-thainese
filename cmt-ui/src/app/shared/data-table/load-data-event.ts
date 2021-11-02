import { SortMetaData } from './sort-meta-data';

export class LoadDataEvent {
	count: number;
	startIndex: number;
	pageSize: number;
	sortField?: string;
	sortOrder?: number;
	multiSortMetaData?: SortMetaData[];
}
