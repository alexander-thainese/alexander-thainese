import { Component, OnInit, Input } from '@angular/core';

@Component({
	selector: 'cmt-collapsible-panel',
	templateUrl: 'collapsible-panel.component.html',
	styles: [
		`
    .cp-card {
      margin-bottom: 10px;
    }
  `
	]
})
export class CollapsiblePanel implements OnInit {
	@Input() header: string;
	visible = false;
	constructor() {}

	ngOnInit() {}

	toggle() {
		this.visible = !this.visible;
	}
}
