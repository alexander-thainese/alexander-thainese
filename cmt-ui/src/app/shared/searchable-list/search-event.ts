import { Observable } from 'rxjs';

export class SearchEvent {
    searchTerm: string;
    result: Observable<any[]>;

    constructor(searchTerm: string) {
        this.searchTerm = searchTerm;
    }
}