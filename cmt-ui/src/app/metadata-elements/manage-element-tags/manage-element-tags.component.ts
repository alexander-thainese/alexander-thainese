import { Component, OnInit, Input, OnDestroy, EventEmitter, Output } from '@angular/core';
import { SchemaElement, MetadataService, TreeViewService } from '../../shared';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Country } from '../../shared/country.model';

export enum TagLevel {
	Schema = 1,
	ElementValue = 2,
	Country = 3
}

export enum ValueTagType {
	Include = 0,
	Exclude = 1
}

@Component({
	selector: 'manage-element-tags',
	templateUrl: './manage-element-tags.component.html',
	styleUrls: ['./manage-element-tags.component.scss']
})
export class ManageElementTagsComponent implements OnInit, OnDestroy {
	@Input() metadataElement: SchemaElement;
	@Input() metadataElements: SchemaElement[];
	@Output() close = new EventEmitter<boolean>();
	selectedTagLevel: TagLevel = TagLevel.Schema;
	selectedTagSchema: string;
	selectedTagElement: SchemaElement;
	selectedTagValueId: string;
	selectedTagCountryId: string;
	tagLevel = TagLevel;

	selectedValueTagType: ValueTagType = ValueTagType.Exclude;
	valueTagType = ValueTagType;
	schemaTags$: Observable<void | any[]>;
	countries$: Observable<void | Country[]>;

	get selectedItems() {
		if (!this.metadataElement) {
			return [];
		} else {
			return this.metadataElement.Children.filter((p) => p.IsSelected === true);
		}
	}
	get elementValueItems() {
		if (!this.selectedTagElement) {
			return [];
		} else {
			return this.selectedTagElement.Children.filter((p) => p.Status === true);
		}
	}


	constructor(private readonly metadataService: MetadataService, private readonly treeViewService: TreeViewService) { }

	ngOnInit() {
		this.schemaTags$ = this.metadataService.getSchemaTags();
		this.countries$ = this.metadataService.getAllCountriesObservable().pipe(take(1));
	}

	addTags() {
		if (!this.selectedItems) {
			return;
		}
		if (this.selectedTagLevel === TagLevel.Schema && !this.selectedTagSchema) {
			return;
		}
		let valueTags: ValueTag[] = this.selectedItems.map((e) => this.createValueTag(e.Id));
		this.metadataService.addValueTags(valueTags).pipe(take(1)).subscribe((result) => {
			if (result === true) {
				this.treeViewService.refreshData('');
				this.closeAddTagsWindow();
			}
		});
	}

	closeAddTagsWindow() {
		this.close.emit(true);
	}

	createValueTag(id: string): ValueTag {
		return {
			ValueId: id,
			SchemaTag: this.selectedTagLevel === TagLevel.Schema ? this.selectedTagSchema : undefined,
			TagElementId: this.selectedTagLevel === TagLevel.ElementValue ? this.selectedTagElement.Id : undefined,
			TagValueId: this.selectedTagLevel === TagLevel.ElementValue ? this.selectedTagValueId : undefined,
			CountryId: this.selectedTagLevel === TagLevel.Country ? this.selectedTagCountryId : undefined,
			Type: this.selectedValueTagType
		};
	}

	ngOnDestroy(): void {
		if (!this.metadataElement || !this.metadataElement.Children) {
			return;
		}
		this.metadataElement.Children.forEach((p) => {
			p.IsSelected = false;
		});
	}
}

export interface ValueTag {
	ValueId: string;
	SchemaTag: string;
	TagValueId: string;
	TagElementId: string;
	CountryId: string;
	Type: ValueTagType;
}

export interface MetadataValueTag {
	ValueId: string;
	SchemaTag: string;
	ElementId: string;
	Type: ValueTagType;
}
