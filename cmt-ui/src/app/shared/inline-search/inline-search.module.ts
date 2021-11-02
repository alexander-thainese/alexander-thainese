import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { CmtInlineSearch } from './inline-search.component';
import { MatIconModule, MatInputModule } from '@angular/material';
import { CommonModule } from '@angular/common';

@NgModule({
	imports: [ BrowserModule, CommonModule, FormsModule, MatIconModule, MatInputModule ],
	exports: [ CmtInlineSearch ],
	declarations: [ CmtInlineSearch ],
	providers: []
})
export class CmtInlineSearchModule {}
