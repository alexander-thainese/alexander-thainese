// import { Component, Renderer, ElementRef, ViewChildren, QueryList } from '@angular/core';
// import { Field, FormlyPubSub, FormlyMessages, FormlyValueChangeEvent } from 'ng2-formly';
// import { SingleFocusDispatcher } from 'ng2-formly/lib/templates';
// import { MatRadioButton } from '@angular/material/radio';

// @Component({
// 	selector: 'formly-field-radio',
// 	template: `
//     <div [formGroup]="form">
//         <label for="">{{templateOptions.label}}</label>
//         <mat-radio-group [(ngModel)]="model" [formControlName]="key">
//           <p *ngFor="let option of templateOptions.options">
//             <mat-radio-button [value]="option.key"
//               (change)="inputChange($event, option.key)" (focus)="onInputFocus()"
//               [disabled]="templateOptions.disabled" #inputElement>{{option.value}}
//             </mat-radio-button>
//           </p>
//         </mat-radio-group>
//         <small class="text-muted">{{templateOptions.description}}</small>
//     </div>`,
// 	queries: { inputComponent: new ViewChildren('inputElement') }
// })
// export class FormlyFieldRadio extends Field {
// 	constructor(fm: FormlyMessages, ps: FormlyPubSub, renderer: Renderer, focusDispatcher: SingleFocusDispatcher) {
// 		super(fm, ps, renderer, focusDispatcher);
// 	}

// 	inputComponent: QueryList<MatRadioButton>;

// 	protected setNativeFocusProperty(newFocusValue: boolean): void {
// 		if (this.inputComponent.length > 0) {
// 			// this.inputComponent.first.focus();
// 		}
// 	}

// 	inputChange(e, val) {
// 		this.model = val;
// 		this.changeFn.emit(new FormlyValueChangeEvent(this.key, this.model));
// 		this.ps.setUpdated(true);
// 	}
// }
