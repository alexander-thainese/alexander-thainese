import {
	Component,
	OnInit,
	Input,
	OnChanges,
	SimpleChanges,
	OnDestroy,
	ViewChild,
	Output,
	EventEmitter
} from '@angular/core';
import { OnSave } from '../on-save.interface';
import { Subscription } from 'rxjs';
import { TreeViewService, MetadataService, SchemaElement } from '../../../index';
import { ExternalSystem } from '../metadata-element-detail/external-system';
import { HttpClient } from '../../../http-client.service';
import { tap, take } from 'rxjs/operators';

@Component({
	selector: 'cmt-metadata-schema-detail',
	templateUrl: 'metadata-schema.component.html'
})
export class CmtMetadataSchemaDetailComponent implements OnInit, OnSave, OnChanges, OnDestroy {
	@Input() selectedObject: SchemaElement;
	subscription: Subscription;
	systems: ExternalSystem[];
	@Input() editMode = false;

	countries: any[];

	visibleAddCountryPopup = false;
	visibleAddTagPopup = false;
	filteredTags: any[];
	allCountries: any[];
	schemaTags: any[];

	constructor(
		private treeViewService: TreeViewService,
		private metadataService: MetadataService,
		private http: HttpClient
	) {}

	ngOnInit() {
		this.subscription = this.treeViewService.detailsSave$.subscribe((save) => {
			if (save) {
				this.onSave();
			} else {
				this.onCancel();
			}
		});
		this.refreshCountries();
		this.refreshSchemaTags();
		this.metadataService.getAllCountries().then((countries) => {
			this.allCountries = countries;
			this.filteredTags = this.allCountries.slice(0);
		});
	}

	public onCancel(): void {}

	public onSave(): void {}

	selectionChanged(system: ExternalSystem, data: any) {
		this.metadataService
			.updateThirdPartySystemsAccess(system, true)
			.catch((error) => (data.source.checked = !data.source.checked));
	}

	get sortedSystems(): ExternalSystem[] {
		if (this.systems) {
			return this.systems.sort((n1, n2) => {
				if (n1.Name > n2.Name) {
					return 1;
				}

				if (n1.Name < n2.Name) {
					return -1;
				}

				return 0;
			});
		} else {
			return this.systems;
		}
	}

	ngOnChanges(changes: SimpleChanges) {
		this.metadataService.getThirdPartySystemsAccess(this.selectedObject.Id, true).then((res) => {
			this.systems = res as ExternalSystem[];
		});
		this.refreshCountries();
		this.refreshSchemaTags();
	}

	get definitionDate(): Date {
		return new Date(this.selectedObject.DefinitionDate);
	}
	get activationDate(): Date {
		return new Date(this.selectedObject.ActivationDate);
	}

	get deactivationDate(): Date {
		return new Date(this.selectedObject.DeactivationDate);
	}

	formatDate(date: Date): string {
		return (
			this.addLeadingZero(date.getDate()) +
			'.' +
			this.addLeadingZero(date.getMonth() + 1) +
			'.' +
			date.getFullYear()
		);
	}

	addLeadingZero(num: number): string {
		let s = '0' + num;
		return s.substr(s.length - 2);
	}

	ngOnDestroy() {
		this.subscription.unsubscribe();
	}

	get allowEdit(): boolean {
		return this.http.isAdmin;
	}

	onUpdateDescription(id: any, description: any): void {
		this.metadataService.updateSchemaDescription(id, description).then(
			(result) => {},
			(error) => {
				console.log('error during save');
			}
		);
	}

	getSchemaStatus(item: SchemaElement) {
		if (item.Status === true) {
			return 'Active';
		} else if (!item.DeactivationDate) {
			return 'New';
		} else {
			return 'Inactive';
		}
	}

	openAddCountryPopup() {
		this.visibleAddCountryPopup = true;
	}

	hideAddCountryPopup() {
		this.visibleAddCountryPopup = false;
	}

	openAddTagPopup() {
		this.visibleAddTagPopup = true;
	}

	hideAddTagPopup() {
		this.visibleAddTagPopup = false;
	}

	searchCountry(query: string) {
		this.filteredTags = this.allCountries.filter((p) => p.Name.toLowerCase().indexOf(query.toLowerCase()) > -1);
	}

	onAddCountry(country) {
		this.metadataService.addSchemaCountry(this.selectedObject.Id, country.Id).then(() => this.refreshCountries());
		this.visibleAddCountryPopup = false;
	}

	onDeleteCountry(country) {
		this.metadataService
			.deleteSchemaCountry(this.selectedObject.Id, country.Id)
			.then(() => this.refreshCountries());
	}

	onAddSchemaTag(tag) {
		this.metadataService.addSchemaTag(this.selectedObject.Id, tag).then(() => this.refreshSchemaTags());
		this.visibleAddTagPopup = false;
	}

	onDeleteSchemaTag(tag) {
		this.metadataService.deleteSchemaTag(this.selectedObject.Id, tag.Value).then(() => this.refreshSchemaTags());
	}

	refreshCountries() {
		this.metadataService
			.getSchemaCountries(this.selectedObject.Id)
			.then((countries) => (this.countries = countries as any[]));
	}

	refreshSchemaTags() {
		this.metadataService
			.getSchemaTags(this.selectedObject.Id)
			.pipe(take(1), tap((schemaTags) => (this.schemaTags = schemaTags as any[])))
			.subscribe();
	}
}
