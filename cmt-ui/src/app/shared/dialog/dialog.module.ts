import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from './dialog.component';
import { MatCardModule, MatInputModule, MatButtonModule } from '@angular/material';

@NgModule({
	imports: [ BrowserModule, FormsModule, MatCardModule, MatInputModule, MatButtonModule ],
	exports: [ DialogComponent ],
	declarations: [ DialogComponent ],
	providers: []
})
export class DialogModule {}
