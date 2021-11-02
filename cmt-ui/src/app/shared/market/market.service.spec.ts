import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { MarketService } from './market.service';
import { CommonModule } from '../common.module';
import { HttpModule } from '@angular/http';
import { HttpClient } from '../http-client.service';
import { MessageService } from '../message/message.service';
import { BrowserDynamicTestingModule } from '@angular/platform-browser-dynamic/testing';

describe('MarketService', () => {
	beforeEach(() =>
		TestBed.configureTestingModule({
			imports: [ HttpModule, CommonModule, BrowserDynamicTestingModule, RouterTestingModule ],
			providers: [ HttpClient, MessageService ]
		})
	);

	it('should be created', () => {
		const service: MarketService = TestBed.get(MarketService);
		expect(service).toBeTruthy();
	});
});
