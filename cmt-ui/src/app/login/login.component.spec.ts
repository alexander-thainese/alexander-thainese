import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginComponent } from './login.component';
import { MatProgressSpinnerModule, MatInputModule, MatButtonModule, MatCardModule } from '@angular/material';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '../shared/http-client.service';
import { HttpModule } from '@angular/http';
import { MessageService } from '../shared/message/message.service';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('LoginComponent', () => {
	let component: LoginComponent;
	let fixture: ComponentFixture<LoginComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [
					BrowserAnimationsModule,
					HttpModule,
					ReactiveFormsModule,
					MatProgressSpinnerModule,
					MatInputModule,
					MatButtonModule,
					MatCardModule,
					RouterTestingModule
				],
				declarations: [ LoginComponent ],
				providers: [ HttpClient, MessageService ]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(LoginComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
