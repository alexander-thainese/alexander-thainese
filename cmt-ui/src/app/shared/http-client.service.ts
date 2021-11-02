import { empty as observableEmpty, of as observableOf, Observable, Subscribable } from 'rxjs';

import { mergeMap, catchError, map } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Headers, Response, RequestOptionsArgs } from '@angular/http';

import { TokenResponse } from './token-response';

import { MessageService } from './message/message.service';
import { environment } from '../../environments/environment';
import { MarketService } from './market/market.service';

@Injectable({
	providedIn: 'root'
})
export class HttpClient {
	private userTokenStorageKey = 'user-token';
	get hasToken(): boolean {
		return true; // this.token && this.token.access_token ? true : false;
	}

	get isAdmin(): boolean {
		return true;
		// if (!this.token || !this.token.roles) {
		// 	return false;
		// } else {
		// 	return this.token.roles.find((r) => r.toLowerCase() === 'admin') !== undefined;
		// }
	}

	get isDataSteward(): boolean {
		if (!this.token || !this.token.roles) {
			return false;
		} else {
			return this.token.roles.find((r) => r.toLowerCase() === 'datasteward') !== undefined;
		}
	}

	get isDev(): boolean {
		if (!this.token || !this.token.roles) {
			return false;
		} else {
			return this.token.roles.find((r) => r.toLowerCase() === 'dev') !== undefined;
		}
	}
	private get token(): TokenResponse {
		const userTokenString = sessionStorage.getItem(this.userTokenStorageKey);
		if (userTokenString) {
			const token = JSON.parse(userTokenString);
			this.expirationDate = new Date(token['.expires']);
			return token;
		} else {
			return undefined;
		}
	}
	private set token(value: TokenResponse) {
		sessionStorage.setItem(this.userTokenStorageKey, JSON.stringify(value));
	}

	private expirationDate: Date;
	public inProgress: boolean;
	public forbidden: boolean;

	constructor(private http: Http, private messageService: MessageService, private router: Router) {
		this.inProgress = false;
		this.forbidden = false;
	}

	getToken(): TokenResponse {
		return this.token;
	}

	createAuthorizationHeader(headers: Headers) {
		headers.append('Authorization', 'Bearer ' + (this.token && this.token.access_token));
		headers.append('Accept', 'application/json');
		headers.append('Content-Type', 'application/json');
	}

	createNoCachingHeader(headers: Headers) {
		headers.append('Cache-control', 'no-cache');
		headers.append('Cache-control', 'no-store');
		headers.append('Cache-control', 'must-revalidate');
		headers.append('Pragma', 'no-cache');
		headers.append('Expires', '0');
	}
	getAvailableMarkets(): Observable<any> {
		if (!this.isTokenValid()) {
			return observableOf();
		}
		return this.get(environment.baseUrl + 'api/user-countries');
	}

	authenticate(forceRecheck?: boolean): Observable<boolean> {
		if (forceRecheck || !this.hasToken || !this.isTokenValid()) {
			return observableOf<boolean>(false);
		} else {
			return observableOf<boolean>(true);
		}
	}

	authenticateWithPassword(username: string, passowrd: string): Observable<boolean> {
		const authenticationResponse = this.http.post(environment.baseUrl + 'api/authorization', {
			userName: username,
			password: passowrd
		});
		return this.processAuthenticationResponse(authenticationResponse);
	}

	get(url: string, options?: RequestOptionsArgs, returnRawData?: boolean): Observable<any> {
		if (this.hasToken && this.isTokenValid()) {
			if (options) {
				let headers = options.headers;
				this.createAuthorizationHeader(headers);
				this.createNoCachingHeader(headers);
				options.headers = headers;
				return this.http.get(url, options).pipe(
					map((response) => {
						this.messageService.sendClearConnectionError();
						if (!returnRawData) {
							return response.json();
						} else {
							return response;
						}
					}),
					catchError((err) => this.handleError(err, this))
				);
			} else {
				let headers = new Headers();
				this.createAuthorizationHeader(headers);
				this.createNoCachingHeader(headers);
				return this.http
					.get(url, {
						headers: headers
					})
					.pipe(
						map((response) => {
							this.messageService.sendClearConnectionError();
							return response.json();
						}),
						catchError((err) => this.handleError(err, this))
					);
			}
		} else {
			return this.authenticate().pipe(
				map((result) => result),
				mergeMap((token) => this.get(url)),
				map((res: Response) => res)
			);
		}
	}

	post(url: string, data: any): Observable<any> {
		if (this.hasToken && this.isTokenValid()) {
			let headers = new Headers();
			this.createAuthorizationHeader(headers);
			return this.http
				.post(url, data, {
					headers: headers
				})
				.pipe(
					map((response) => {
						this.messageService.sendClearConnectionError();
						return response.json();
					}),
					catchError((err) => this.handleError(err, this))
				);
		} else {
			return this.authenticate().pipe(map((result) => result), mergeMap((token) => this.post(url, data)));
		}
	}

	public isTokenValid(): boolean {
		return true; // this.expirationDate && this.expirationDate.getTime() > new Date(Date.now()).getTime();
	}

	private processAuthenticationResponse(authenticationResponse: Observable<Response>): Observable<boolean> {
		return authenticationResponse.pipe(
			map((result) => {
				this.messageService.sendClearConnectionError();
				if (result.status === 401) {
					this.forbidden = true;
					return false;
				} else if (!result.ok) {
					this.messageService.showMessage({
						severity: 'error',
						summary: 'Authorization service is unavailable'
					});
					return false;
				}
				this.token = result.json() as TokenResponse;
				if (!this.token) {
					return false;
				}
				this.inProgress = false;
				return true;
			}),
			catchError((err) => this.handleError(err, this))
		);
	}

	private handleError(error: any, source: HttpClient): Observable<boolean> {
		if (error.status && error.status === 401) {
			source.inProgress = false;
		} else if (error.status && error.status === 403) {
			source.router.navigate([ '/login' ]);
			source.inProgress = false;
		} else if (error.status === 406) {
			source.messageService.showMessage({ severity: 'warning', summary: error.json().Message });
		} else if (error.status === 0) {
			source.messageService.showMessage({
				severity: 'error',
				summary: 'Limited internet connectivity',
				isConnectionError: true
			});
			source.inProgress = false;
		} else {
			try {
				source.messageService.showMessage({ severity: 'error', summary: error.json().Message });
			} catch (ex) {
				source.messageService.showMessage({ severity: 'error', summary: 'Unhandled error occured' });
			}
			source.inProgress = false;
		}
		return observableOf(false);
	}
}
