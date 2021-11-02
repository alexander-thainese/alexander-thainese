import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { Observable, of } from 'rxjs';
import { MarketService } from '../shared/market/market.service';
import { tap } from 'rxjs/operators';

@Component({
	selector: 'cmt-header',
	templateUrl: './cmt-header.component.html',
	styleUrls: [ './cmt-header.component.scss' ]
})
export class CmtHeaderComponent implements OnInit {
	@Output() public sidenavToggle = new EventEmitter();
	filteredOptions$: Observable<any>;
	public selectedMarket: string;
	constructor(private readonly marketService: MarketService) {}

	ngOnInit() {
		this.filteredOptions$ = this.marketService.availableMarkets$.pipe(
			tap(() => (this.selectedMarket = this.marketService.marketCode))
		);
	}

	public onToggleSidenav() {
		this.sidenavToggle.emit();
	}

	public countryChanged(marketCode: string) {
		this.marketService.notifySelectedMarketChanged(marketCode);
	}
}
