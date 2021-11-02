import { map } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { CanActivate, Router, CanLoad } from '@angular/router';
import { Observable } from 'rxjs';
import { MarketService } from './market.service';

@Injectable()
export class MarketGuard implements CanActivate {
	constructor(private router: Router, private marketService: MarketService) {}

	canActivate(): Observable<boolean> {
		// here check if this is first time call. If not return
		// simple boolean based on user object from authService
		// otherwise:
		return this.marketService.isUserInAnyMarket().pipe(
			map((result) => {
				if (!result) {
					this.router.navigate([ '/login' ]);
					return false;
				} else {
					return true;
				}
			})
		);
	}
}
