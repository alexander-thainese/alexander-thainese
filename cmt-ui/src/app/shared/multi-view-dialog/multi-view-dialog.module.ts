import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MultiViewDialogComponent } from './multi-view-dialog.component';
import { DialogViewComponent } from './dialog-view/dialog-view.component';
import { MatCardModule, MatIconModule } from '@angular/material';

@NgModule({
	imports: [ BrowserModule, FormsModule, MatCardModule, MatIconModule ],
	exports: [ MultiViewDialogComponent, DialogViewComponent ],
	declarations: [ MultiViewDialogComponent, DialogViewComponent ],
	providers: []
})
export class MultiViewDialogModule {}
