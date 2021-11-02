import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { SearchableListComponent } from './searchable-list.component';
import { CmtInlineSearchModule } from '../inline-search/inline-search.module';
import { CommonModule } from '../common.module';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';

@NgModule({
	imports: [ BrowserModule, FormsModule, CmtInlineSearchModule, CommonModule, PerfectScrollbarModule ],
	exports: [ SearchableListComponent ],
	declarations: [ SearchableListComponent ],
	providers: []
})
export class SearchableListModule {}
