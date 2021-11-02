import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { PaginationComponent } from './pagination.component';
import { MatSelectModule, MatIconModule } from '@angular/material';

@NgModule({
	imports: [ BrowserModule, FormsModule, MatSelectModule, MatIconModule ],
	exports: [ PaginationComponent ],
	declarations: [ PaginationComponent ],
	providers: []
})
export class PaginationModule {}
