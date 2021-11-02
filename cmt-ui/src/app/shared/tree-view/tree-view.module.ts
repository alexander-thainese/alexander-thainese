import { AddTagsPopupModule } from './../add-tags-popup/add-tags-popup.module';
import { TagEditorComponent } from './../tag-editor/tag-editor.component';
import { PopupModule } from './../popup/popup.module';
import { SchemaElementSelectionComponent } from './element-detail/schema-detail/element-selection/element-selection.component';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CmtTreeViewComponent } from './tree-view.component';
import { DialogModule } from '../dialog/dialog.module';
import { AppModule } from '../../app.module';
import { CollapsiblePanel as CollapsiblePanelComponent } from '../collapsible-panel/collapsible-panel.component';
import { CmtTreelistElementComponent } from './treelist-element';
import {
	CmtElementDetailComponent,
	CmtMetadataElementDetailComponent,
	CmtMetadataSchemaDetailComponent,
	CmtMetadataValueDetailComponent
} from './element-detail';
import { SpinnerModule } from '../index';
import { PaginationModule } from '../pagination/pagination.module';
import { CmtInlineEditComponent } from '../inline-edit';
import { CmtInlineSearchModule } from '../inline-search/inline-search.module';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { MomentModule } from 'ngx-moment';
import { CmtValueAttributeTagComponent } from '../value-attribute-tag';
import {
	MatProgressSpinnerModule,
	MatCardModule,
	MatIconModule,
	MatMenuModule,
	MatCheckboxModule,
	MatInputModule,
	MatSelectModule,
	MatTooltipModule
} from '@angular/material';
// tslint:disable-next-line: max-line-length
import { ElementTypeChangePopupComponent } from './element-detail/metadata-element-detail/element-type-change-popup/element-type-change-popup.component';
import { HasAttributesPipe } from './element-detail/has-attributes-pipe.pipe';

@NgModule({
	imports: [
		BrowserModule,
		DialogModule,
		FormsModule,
		SpinnerModule,
		PaginationModule,
		PerfectScrollbarModule,
		MatProgressSpinnerModule,
		CmtInlineSearchModule,
		MatCardModule,
		MatIconModule,
		MatMenuModule,
		MatCheckboxModule,
		MatInputModule,
		MatSelectModule,
		PopupModule,
		AddTagsPopupModule,
		MomentModule,
		MatTooltipModule
	],
	declarations: [
		CmtTreeViewComponent,
		CollapsiblePanelComponent,
		CmtTreelistElementComponent,
		CmtElementDetailComponent,
		CmtMetadataElementDetailComponent,
		CmtMetadataSchemaDetailComponent,
		CmtMetadataValueDetailComponent,
		CmtInlineEditComponent,
		SchemaElementSelectionComponent,
		TagEditorComponent,
		ElementTypeChangePopupComponent,
		CmtValueAttributeTagComponent,
		HasAttributesPipe
	],
	exports: [ CmtTreeViewComponent, CmtMetadataElementDetailComponent ]
})
export class CmtTreeViewModule {}
