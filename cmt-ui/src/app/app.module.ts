import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule, FormGroup } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { ErrorHandler } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';

import {
	MatCommonModule,
	MatCardModule,
	MatProgressSpinnerModule,
	MatInputModule,
	MatIconModule,
	MatToolbarModule,
	MatSelectModule
} from '@angular/material';

import { AppComponent } from './app.component';

import { CmtSchemaTreeModule } from './schema-tree';
import { CmtMetadataElementsModule } from './metadata-elements';
import { APP_BASE_HREF } from '@angular/common';
import { routing } from './app.routing';
import {
	CustomErrorHandler,
	ERROR_HANDLER_PROVIDERS,
	ERROR_HANDLER_OPTIONS
} from './shared/error/custom-error-handler';
import { ErrorHandlerService } from './shared/error/error-handler.service';
import { MessageService } from './shared/message/message.service';
import { MessageModule } from './shared/message/message.module';
import { HttpClient } from './shared/http-client.service';
import { AuthGuard, AuthIsAdmin } from './shared/auth.guard';
import { CmtUnauthorizedComponent } from './shared/unauthorized';

import { MomentModule } from 'ngx-moment';
import { LoginModule } from './login/login.module';
import { CmtHeaderComponent } from './cmt-header/cmt-header.component';
import { MarketGuard } from './shared/market/market.guard';

@NgModule({
	declarations: [ AppComponent, CmtUnauthorizedComponent, CmtHeaderComponent ],
	imports: [
		BrowserAnimationsModule,
		BrowserModule,
		FormsModule,
		HttpModule,
		CmtSchemaTreeModule,
		CmtMetadataElementsModule,
		MessageModule,
		routing,
		MatIconModule,
		MomentModule,
		MatCommonModule,
		MatCardModule,
		MatInputModule,
		MatSelectModule,
		MatProgressSpinnerModule,
		MatToolbarModule,
		FlexLayoutModule,
		LoginModule
	],
	exports: [ MatCommonModule ],
	providers: [
		{ provide: APP_BASE_HREF, useValue: '/' },
		MessageService,
		HttpClient,
		AuthGuard,
		MarketGuard,
		AuthIsAdmin,
		ErrorHandlerService,
		ERROR_HANDLER_PROVIDERS,
		{
			provide: ERROR_HANDLER_OPTIONS,
			useValue: {
				rethrowError: false
			}
		}
	],
	bootstrap: [ AppComponent ]
})
export class AppModule {}
