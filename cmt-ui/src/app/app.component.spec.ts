/* tslint:disable:no-unused-variable */
import { AppComponent } from './app.component';

import { TestBed } from '@angular/core/testing';

import { By } from '@angular/platform-browser';
import { Component, Input } from '@angular/core';
import { HttpClient } from './shared/http-client.service';
import { MessageService } from './shared/message/message.service';
import { HttpModule } from '@angular/http';
import { RouterTestingModule } from '@angular/router/testing';

@Component({ selector: 'message', template: '' })
class MessageStubComponent {
	@Input() autoclose: boolean;
}
@Component({ selector: 'cmt-header', template: '' })
class CmtHearderStubComponent {}

describe('AppComponent with TCB', function() {
	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [ HttpModule, RouterTestingModule ],
			declarations: [ AppComponent, MessageStubComponent, CmtHearderStubComponent ],
			providers: [ HttpClient, MessageService ]
		});
	});

	it('should instantiate component', () => {
		let fixture = TestBed.createComponent(AppComponent);
		expect(fixture.componentInstance instanceof AppComponent).toBe(true, 'should create AppComponent');
	});
});
