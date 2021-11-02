import { Component } from '@angular/core';
import { HttpClient } from './shared/http-client.service';

import * as moment from 'moment';
import 'moment/min/locales.min';

@Component({
	selector: 'cmt',
	templateUrl: 'app.component.html',
	styleUrls: [ 'app.component.css' ]
})
export class AppComponent {
	constructor(private http: HttpClient) {
		moment.locale(window.navigator.language);
	}

	get isAuthenticated() {
		return this.http.isTokenValid();
	}
}
