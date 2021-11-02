import { Pipe, PipeTransform } from '@angular/core';
import { ParentValueAttribute } from '../../parent-value-attribute';

@Pipe({
	name: 'hasAttributesPipe'
})
export class HasAttributesPipe implements PipeTransform {
	transform(parentValueId: string, parentValueAttributes?: ParentValueAttribute[]): any {
		if (!parentValueAttributes) {
			return false;
		}
		return parentValueAttributes.some((p) => p.ParentId === parentValueId);
	}
}
