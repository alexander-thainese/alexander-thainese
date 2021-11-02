/**
 * Created by gitsad on 18.11.16.
 */
import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AddTagsPopupComponent } from './add-tags-popup.component';
import { MatDividerModule, MatCardModule } from '@angular/material';

@NgModule({
	imports: [ CommonModule, FormsModule, MatDividerModule, MatCardModule ],
	declarations: [ AddTagsPopupComponent ],
	exports: [ AddTagsPopupComponent ]
})
export class AddTagsPopupModule {}
