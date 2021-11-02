import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { PopupModule } from './../shared/popup/popup.module';
import { NgModule } from '@angular/core';
import { CmtTreeViewModule } from '../shared/tree-view';
import { CmtSchemaTreeComponent } from './schema-tree.component';
import { routing } from './schema-tree.routing';
import { MatSelectModule, MatInputModule } from '@angular/material';

@NgModule({
	imports: [ CmtTreeViewModule, PopupModule, routing, BrowserModule, FormsModule, MatSelectModule, MatInputModule ],
	exports: [],
	declarations: [ CmtSchemaTreeComponent ],
	providers: []
})
export class CmtSchemaTreeModule {}
