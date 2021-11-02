import { Injectable } from '@angular/core';
import { HttpClient } from '../http-client.service';
import { tap, map } from 'rxjs/operators';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { Market } from './market.model';

const selectedMarketStorageKey = 'selected-market';

@Injectable({
	providedIn: 'root'
})
export class MarketService {
	private marketChanged$ = new BehaviorSubject<Market>(undefined);
	private _availableMarkets: Market[] = [];
	availableMarkets$ = new Subject<Market[]>();
	marketChanged = this.marketChanged$.asObservable();

	get availableMarkets(): Market[] {
		return this._availableMarkets;
	}
	set availableMarkets(markets: Market[]) {
		this._availableMarkets = markets;
		this.availableMarkets$.next(markets);
	}

	get marketCode(): string {
		let code = localStorage.getItem(selectedMarketStorageKey);
		if (!code) {
			code = this.availableMarkets && this.availableMarkets.length > 0 && this.availableMarkets[0].Code;
			this.marketCode = code;
		}
		return code;
	}
	set marketCode(code: string) {
		localStorage.setItem(selectedMarketStorageKey, code);
		const market = this.availableMarkets.find((m) => m.Code === code);
		this.marketChanged$.next(market);
	}

	constructor(private http: HttpClient) {}

	public isUserInAnyMarket(): Observable<boolean> {
		return this.http.getAvailableMarkets().pipe(
			tap((markets) => {
				this.availableMarkets = markets || [];
				const market = this.availableMarkets.find((p) => p.Code === this.marketCode);
				if (!market) {
					this.marketCode = '';
				} else {
					this.marketChanged$.next(market);
				}
			}),
			map((markets) => markets.length > 0)
		);
	}

	public notifySelectedMarketChanged(code: string) {
		this.marketCode = code;
		window.location.reload();
	}
}
