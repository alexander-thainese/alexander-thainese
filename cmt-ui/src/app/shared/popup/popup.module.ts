import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { PopupComponent } from './popup.component';
import { PopupListService } from './popup.service';
import { MatCardModule, MatInputModule, MatButtonModule } from '@angular/material';

@NgModule({
	imports: [ BrowserModule, FormsModule, MatCardModule, MatInputModule, MatButtonModule ],
	exports: [ PopupComponent ],
	declarations: [ PopupComponent ],
	providers: [ PopupListService ]
})
export class PopupModule {}
