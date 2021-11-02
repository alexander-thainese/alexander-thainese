import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { WizardComponent } from './wizard.component';
import { WizardStepComponent } from './wizard-step/wizard-step.component';
import { MatCardModule, MatIconModule, MatTooltipModule } from '@angular/material';

@NgModule({
	imports: [ BrowserModule, MatCardModule, MatIconModule, MatTooltipModule ],
	exports: [ WizardComponent, WizardStepComponent ],
	declarations: [ WizardComponent, WizardStepComponent ],
	providers: []
})
export class WizardModule {}
