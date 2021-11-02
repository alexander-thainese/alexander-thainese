import { Component, Input } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';

@Component({
	selector: 'wizard-step',
	templateUrl: 'wizard-step.component.html',
	styleUrls: [ 'wizard-step.component.css' ],
	viewProviders: [ MatIconRegistry ]
})
export class WizardStepComponent {
	completed: boolean;
	visible: boolean;
	@Input() sequence: number;
	@Input() title: string;
	@Input() isFirstStep: boolean;
	@Input() isLastStep: boolean;

	constructor() {
		this.completed = false;
		this.visible = false;
		this.isFirstStep = false;
		this.isLastStep = false;
	}
}
