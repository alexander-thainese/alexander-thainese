import { MetadataService } from './metadata.service';
import { MarketService } from './market/market.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';

describe('Metadata service test', function() {
	let httpClientSpy: { post: jasmine.Spy };
	let metadataService: MetadataService;
	let messageServiceSpy: { showMessage: jasmine.Spy };
	let fakePost = (url: string, body: {}): Observable<boolean> => {
		return of(false);
	};

	beforeEach(() => {
		// TODO: spy on other methods too
		httpClientSpy = jasmine.createSpyObj('HttpClient', [ 'post' ]);
		messageServiceSpy = jasmine.createSpyObj('MessageService', [ 'showMessage' ]);
		metadataService = new MetadataService(
			<any>httpClientSpy,
			<any>messageServiceSpy,
			new MarketService(<any>httpClientSpy)
		);
	});

	it('should post UpdateElementType', () => {
		const elementId = 'element-id';
		const typeId = 'element-typeId';
		fakePost = (url, body): Observable<any> => {
			expect(url).toEqual(environment.baseUrl + 'api/AdminMetadata/UpdateElementType');
			expect(body).toEqual({ id: elementId, typeId: typeId });
			return of(true);
		};

		httpClientSpy.post.and.callFake(fakePost);
		messageServiceSpy.showMessage.and.callFake(() => {});

		metadataService.updateElementType(elementId, typeId).subscribe(() => {}, fail);
		expect(httpClientSpy.post.calls.count()).toBe(1, 'one call');
		expect(messageServiceSpy.showMessage.calls.count()).toBe(0, 'never called');
	});
});
