import { map } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { HttpClient } from './http-client.service';
import { Observable, of } from 'rxjs';

@Injectable()
export class AuthGuard implements CanActivate {
	constructor(private router: Router, private http: HttpClient) {}

	canActivate(): Observable<boolean> {
		// here check if this is first time call. If not return
		// simple boolean based on user object from authService
		// otherwise:
		return of(true);
		// return this.http.authenticate().pipe(
		// 	map((result) => {
		// 		if (!result) {
		// 			this.router.navigate([ '/login' ]);
		// 			return false;
		// 		}
		// 		return this.http.hasToken;
		// 	})
		// );
	}
}

@Injectable()
export class AuthIsAdmin implements CanActivate {
	constructor(private router: Router, private http: HttpClient) {}

	canActivate(): Observable<boolean> {
		// here check if this is first time call. If not return
		// simple boolean based on user object from authService
		// otherwise:
		return of(this.http.isAdmin);
		// return this.http.authenticate().pipe(
		// 	map((token) => {
		// 		return this.http.isAdmin;
		// 	})
		// );
	}
}

@Injectable()
export class IsDataStuart implements CanActivate {
	constructor(private router: Router, private http: HttpClient) {}

	canActivate(): Observable<boolean> {
		// here check if this is first time call. If not return
		// simple boolean based on user object from authService
		// otherwise:

		return this.http.authenticate().pipe(
			map((token) => {
				console.log('1');
				return this.http.authenticate ? true : false;
			})
		);
	}
}
