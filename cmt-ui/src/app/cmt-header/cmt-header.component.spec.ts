import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CmtHeaderComponent } from './cmt-header.component';
import { MatIconModule, MatSelectModule, MatToolbarModule } from '@angular/material';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { HttpClient } from '../shared/http-client.service';
import { MessageService } from '../shared/message/message.service';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('CmtHeaderComponent', () => {
	let component: CmtHeaderComponent;
	let fixture: ComponentFixture<CmtHeaderComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [
					BrowserAnimationsModule,
					CommonModule,
					FormsModule,
					HttpModule,
					RouterTestingModule,
					MatIconModule,
					MatSelectModule,
					MatToolbarModule
				],
				declarations: [ CmtHeaderComponent ],
				providers: [ HttpClient, MessageService ]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(CmtHeaderComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
