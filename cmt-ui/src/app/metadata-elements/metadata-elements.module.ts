import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { PopupModule } from './../shared/popup/popup.module';
import { NgModule } from '@angular/core';
import { CmtTreeViewModule } from '../shared/tree-view';
import { TreeViewService, MetadataService } from '../shared';
import { CmtMetadataElementsComponent } from './metadata-elements.component';
import { routing } from './metadata-elements.routing';
import {
	MatSelectModule,
	MatInputModule,
	MatIconModule,
	MatMenuModule,
	MatCheckboxModule,
	MatRadioModule,
	MatButtonModule
} from '@angular/material';
import { MarketGuard } from '../shared/market/market.guard';
import { ManageElementTagsComponent } from './manage-element-tags/manage-element-tags.component';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { CmtInlineSearchModule } from '../shared/inline-search/inline-search.module';
import { FlexLayoutModule } from '@angular/flex-layout';

@NgModule({
	imports: [
		CmtTreeViewModule,
		routing,
		PopupModule,
		FormsModule,
		BrowserModule,
		MatSelectModule,
		MatInputModule,
		MatIconModule,
		MatCheckboxModule,
		MatRadioModule,
		MatMenuModule,
		MatButtonModule,
		CmtTreeViewModule,
		PerfectScrollbarModule,
		CmtInlineSearchModule,
		FlexLayoutModule
	],
	exports: [],
	declarations: [ CmtMetadataElementsComponent, ManageElementTagsComponent ],
	providers: [ TreeViewService, MetadataService, MarketGuard ]
})
export class CmtMetadataElementsModule {}
