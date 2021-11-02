import { NgModule } from '@angular/core';
import { CommonModule as AngularCommonModule } from '@angular/common';

import { TemplateWrapper } from './template-wrapper.directive';
import { ContentTemplateDirective } from './content-template.directive';
import { MatCardModule, MatProgressSpinnerModule } from '@angular/material';
import { FileUploadComponent } from './file-upload/file-upload.component';

@NgModule({
	imports: [ AngularCommonModule, MatCardModule, MatProgressSpinnerModule ],
	exports: [ MatCardModule, TemplateWrapper, ContentTemplateDirective ],
	declarations: [ TemplateWrapper, ContentTemplateDirective, FileUploadComponent ],
	providers: []
})
export class CommonModule {}
