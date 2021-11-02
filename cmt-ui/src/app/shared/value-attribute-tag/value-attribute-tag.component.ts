import { Component, OnInit, Input } from '@angular/core';
import { ParentValueAttribute } from '../parent-value-attribute';

@Component({
	selector: 'cmt-value-attribute-tag',
	styleUrls: [ 'value-attribute-tag.component.css' ],
	templateUrl: 'value-attribute-tag.component.html'
})
export class CmtValueAttributeTagComponent implements OnInit {
	@Input() parentValueId: string;
	@Input() parentsValues: ParentValueAttribute[];

	tags: ParentValueAttribute[];

	ngOnInit(): void {
		if (this.parentsValues && this.parentValueId) {
			this.tags = this.parentsValues.filter((o) => o.ParentId === this.parentValueId);
			if (this.tags && this.tags.length > 0) {
				this.tags = this.tags.sort(
					(a, b) => (a.ElementType < b.ElementType ? -1 : a.ElementType > b.ElementType ? 1 : 0)
				);
			}
		}
	}
}
