// import { NgModule } from '@angular/core';
// import { BrowserModule } from '@angular/platform-browser';
// import { ReactiveFormsModule } from '@angular/forms';
// import { TemplateDirectives } from './templates';
// import { Injectable } from '@angular/core';
// import { FormlyMessages, FormlyConfig, FormlyModule } from 'ng2-formly';

// @Injectable()
// export class FormlyMaterial {
// 	constructor(fc: FormlyConfig, fm: FormlyMessages) {
// 		fm.addStringMessage('required', 'This field is required.');
// 		fm.addStringMessage('invalidEmailAddress', 'Invalid Email Address');
// 		fm.addStringMessage('maxlength', 'Maximum Length Exceeded.');
// 		fm.addStringMessage('minlength', 'Should have atleast 2 Characters');

// 		TemplateDirectives.forEach((type) => fc.setType(type));
// 	}
// }

// @NgModule({
// 	declarations: TemplateDirectives.map((type) => type.component),
// 	entryComponents: TemplateDirectives.map((type) => type.component),
// 	providers: [ FormlyMaterial ],
// 	imports: [ BrowserModule, ReactiveFormsModule, FormlyModule ]
// })
// export class FormlyMaterialModule {}
