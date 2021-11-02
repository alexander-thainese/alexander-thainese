import { CmtInlineEditComponent } from './inline-edit.component';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';
import { RouterTestingModule } from '@angular/router/testing';
import { MatInputModule, MatSelectModule } from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('CmtInlineEditComponent', function() {
	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [
				BrowserAnimationsModule,
				FormsModule,
				HttpModule,
				RouterTestingModule,
				MatInputModule,
				MatSelectModule
			],
			declarations: [ CmtInlineEditComponent ]
		});
	});

	it('should instantiate component', () => {
		let fixture = TestBed.createComponent(CmtInlineEditComponent);
		expect(fixture.componentInstance instanceof CmtInlineEditComponent).toBe(
			true,
			'should create CmtInlineEditComponent'
		);
	});
});
