import { map, catchError } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ErrorHandlerService } from '../../shared/error/error-handler.service';
import { HttpClient } from '../../shared/http-client.service';

@Injectable()
export class PopupListService {
	private getS3FilesUrl: string = environment.baseUrl + 'api/import/S3Files';

	constructor(private http: HttpClient, private errorHandlerService: ErrorHandlerService) {}

	getS3Files(): Observable<any[]> {
		let url = this.getS3FilesUrl;

		return this.http.get(url).pipe(map((response) => response as any[]), catchError(this.handleError));
	}

	downloadFileFromS3(fullName: string): Observable<string> {
		let url = environment.baseUrl + 'api/import/Download?fileKey=' + encodeURIComponent(fullName);
		return this.http.get(url).pipe(map((response) => response as string, catchError(this.handleError)));
	}

	handleError(error: any) {
		this.errorHandlerService.logError(error);
		return Promise.reject(error);
	}
}
