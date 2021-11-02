import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LoginRoutingModule } from './login-routing.module';
import { LoginComponent } from './login.component';
import { MatCardModule, MatInputModule, MatProgressSpinnerModule, MatButtonModule } from '@angular/material';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
	declarations: [ LoginComponent ],
	imports: [
		CommonModule,
		BrowserAnimationsModule,
		FormsModule,
		ReactiveFormsModule,
		LoginRoutingModule,
		MatCardModule,
		MatButtonModule,
		MatInputModule,
		MatProgressSpinnerModule
	]
})
export class LoginModule {}
