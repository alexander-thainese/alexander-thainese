import { distinctUntilChanged, debounceTime } from 'rxjs/operators';
import {
	Component,
	OnInit,
	OnChanges,
	AfterViewInit,
	ContentChild,
	Input,
	Output,
	EventEmitter,
	TemplateRef,
	SimpleChanges
} from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { SearchEvent } from './search-event';

@Component({
	selector: 'searchable-list',
	templateUrl: 'searchable-list.component.html',
	styleUrls: [ 'searchable-list.component.css' ]
})
export class SearchableListComponent implements OnInit, OnChanges, AfterViewInit {
	@Input() caption: string;
	@Input() height: number = 300;
	@Output() selectedItem: any;
	@Output() click: EventEmitter<any> = new EventEmitter();
	@Output() selectionChanged: EventEmitter<any> = new EventEmitter();
	@Output() search = new EventEmitter<SearchEvent>();
	selectedItemId: string = '';

	@ContentChild(TemplateRef) itemTemplate: TemplateRef<any>;
	items = new Subject<any[]>();
	items$ = this.items.asObservable();

	@Input() idColumnName: string = 'Id';
	@Input() showSearchBox: boolean = true;
	@Input() showHeader: boolean = true;

	contentHeight;
	private searchTerms = new Subject<string>();
	private searchTerm = '';
	@Input() elementTrack: any = (index, item) => undefined;

	constructor() {}

	ngOnInit(): void {
		this.recalculateHeight(this.height);
		this.searchTerms.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
			this.searchTerm = term;
			this.performSearch(term).subscribe((data) => this.items.next(data));
		});
	}

	ngAfterViewInit(): void {
		this.onSearch('');
	}

	ngOnChanges(changes: SimpleChanges) {
		let newHeight: number = this.height;
		let heightChanged: boolean = false;

		for (let propName in changes) {
			if (propName == 'height') {
				heightChanged = true;
				newHeight = changes[propName].currentValue;
			}
		}

		if (heightChanged) this.recalculateHeight(newHeight);
	}

	private onClick(event, item) {
		this.selectedItem = item;
		this.selectedItemId = item[this.idColumnName];
		this.click.emit(event);
		this.selectionChanged.emit(item);
	}

	private onSearch(event) {
		this.searchTerms.next(event);
	}

	private recalculateHeight(height: number) {
		this.contentHeight = height - 40 + 'px';
	}

	private performSearch(searchTerm: string): Observable<any[]> {
		let event = new SearchEvent(searchTerm);
		this.search.emit(event);
		return event.result;
	}

	public refresh() {
		this.performSearch(this.searchTerm).subscribe((data) => this.items.next(data));
	}
}
