import { Component, ContentChildren, QueryList, AfterContentInit, Output, EventEmitter } from '@angular/core';

import { WizardStepComponent } from './wizard-step/wizard-step.component';

@Component({
	selector: 'wizard',
	templateUrl: 'wizard.component.html',
	styleUrls: [ 'wizard.component.css' ]
})
export class WizardComponent implements AfterContentInit {
	@Output() finish: EventEmitter<any> = new EventEmitter<any>();
	@ContentChildren(WizardStepComponent) wizardSteps: QueryList<WizardStepComponent>;
	currentStep: WizardStepComponent;

	constructor() {}

	ngAfterContentInit() {
		this.currentStep = this.wizardSteps.first;
		this.currentStep.visible = true;
	}

	onNextStep() {
		this.currentStep.completed = true;
		this.currentStep = this.wizardSteps.filter((o) => o.sequence == this.currentStep.sequence + 1)[0];
		this.currentStep.visible = true;
	}

	onPreviousStep() {
		this.currentStep.completed = false;
		this.currentStep.visible = false;
		this.currentStep = this.wizardSteps.filter((o) => o.sequence == this.currentStep.sequence - 1)[0];
		this.currentStep.visible = true;
		this.currentStep.completed = false;
	}

	onFinish() {
		this.finish.emit(true);
	}
}
