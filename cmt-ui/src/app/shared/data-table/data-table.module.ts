import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { DataTableComponent } from './data-table.component';
import { DataTableColumnComponent } from './data-table-column.component';
import { ColumnDataTemplateComponent } from './column-data-template.component';
import { ColumnHeaderTemplateComponent } from './column-header-template.component';
import { PaginationModule } from '../pagination/pagination.module';
import { MatIconModule, MatProgressSpinnerModule } from '@angular/material';
import { CommonModule } from '../common.module';

@NgModule({
	imports: [ CommonModule, BrowserModule, PaginationModule, MatIconModule, MatProgressSpinnerModule ],
	exports: [ DataTableComponent, DataTableColumnComponent ],
	declarations: [
		DataTableComponent,
		DataTableColumnComponent,
		ColumnDataTemplateComponent,
		ColumnHeaderTemplateComponent
	],
	providers: []
})
export class DataTableModule {}
