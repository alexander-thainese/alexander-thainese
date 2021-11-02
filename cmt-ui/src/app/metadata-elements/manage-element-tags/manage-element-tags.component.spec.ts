import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ManageElementTagsComponent, ValueTagType, ValueTag, TagLevel } from './manage-element-tags.component';
import { MatRadioModule, MatSelectModule, MatCheckboxModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { MetadataService, SchemaElement, TreeViewService } from '../../shared';
import { HttpModule } from '@angular/http';
import { MessageService } from '../../shared/message/message.service';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';

describe('ManageElementTagsComponent', () => {
	let component: ManageElementTagsComponent;
	let fixture: ComponentFixture<ManageElementTagsComponent>;
	let metadataServiceSpy: jasmine.SpyObj<MetadataService>;

	beforeEach(() => {
		const spy = jasmine.createSpyObj('MetadataService', [ 'getSchemaTags', 'getAllCountriesObservable' ]);
		TestBed.configureTestingModule({
			imports: [
				FormsModule,
				HttpModule,
				HttpClientTestingModule,
				MatRadioModule,
				MatSelectModule,
				MatCheckboxModule,
				RouterTestingModule
			],
			declarations: [ ManageElementTagsComponent ],
			providers: [ { provide: MetadataService, useValue: spy }, MessageService, TreeViewService ]
		}).compileComponents();
		metadataServiceSpy = TestBed.get(MetadataService);
		metadataServiceSpy.getSchemaTags.and.returnValue(of([]));
		metadataServiceSpy.getAllCountriesObservable.and.returnValue(of([]));
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(ManageElementTagsComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should create value tag for GENERIC schema with include type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'GENERIC';
		component.selectedTagLevel = TagLevel.Schema;
		component.selectedValueTagType = ValueTagType.Include;
		component.selectedTagElement = new SchemaElement();
		component.selectedTagCountryId = 'US';
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toBeUndefined();
		expect(valueTag.TagValueId).toBeUndefined();
		expect(valueTag.CountryId).toBeUndefined();
		expect(valueTag.SchemaTag).toEqual('GENERIC');
		expect(valueTag.Type).toEqual(ValueTagType.Include, 'ValueTagType should be include');
	});

	it('should create value tag for MAP schema with exclude type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'MAP';
		component.selectedTagLevel = TagLevel.Schema;
		component.selectedValueTagType = ValueTagType.Exclude;
		component.selectedTagCountryId = 'US';
		component.selectedTagElement = new SchemaElement();
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toBeUndefined();
		expect(valueTag.TagValueId).toBeUndefined();
		expect(valueTag.CountryId).toBeUndefined();
		expect(valueTag.SchemaTag).toEqual('MAP');
		expect(valueTag.Type).toEqual(ValueTagType.Exclude, 'ValueTagType should be exclude');
	});

	it('should create value tag for specified value with exclude type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'MAP';
		component.selectedTagLevel = TagLevel.ElementValue;
		component.selectedValueTagType = ValueTagType.Exclude;
		component.selectedTagCountryId = 'US';
		component.selectedTagElement = new SchemaElement();
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toEqual('tag element id');
		expect(valueTag.TagValueId).toEqual('tag value id');
		expect(valueTag.CountryId).toBeUndefined();
		expect(valueTag.SchemaTag).toBeUndefined();
		expect(valueTag.Type).toEqual(ValueTagType.Exclude, 'ValueTagType should be exclude');
	});

	it('should create value tag for specified value with include type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'MAP';
		component.selectedTagLevel = TagLevel.ElementValue;
		component.selectedValueTagType = ValueTagType.Include;
		component.selectedTagCountryId = 'US';
		component.selectedTagElement = new SchemaElement();
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toEqual('tag element id');
		expect(valueTag.TagValueId).toEqual('tag value id');
		expect(valueTag.CountryId).toBeUndefined();
		expect(valueTag.SchemaTag).toBeUndefined();
		expect(valueTag.Type).toEqual(ValueTagType.Include, 'ValueTagType should be include');
	});

	it('should create country tag for specified value with include type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'MAP';
		component.selectedTagLevel = TagLevel.Country;
		component.selectedValueTagType = ValueTagType.Include;
		component.selectedTagCountryId = 'PL';
		component.selectedTagElement = new SchemaElement();
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toBeUndefined();
		expect(valueTag.TagValueId).toBeUndefined();
		expect(valueTag.CountryId).toBe('PL');
		expect(valueTag.SchemaTag).toBeUndefined();
		expect(valueTag.Type).toEqual(ValueTagType.Include, 'ValueTagType should be include');
	});

	it('should create country tag for specified value with exclude type', () => {
		const valueId = 'some id';
		component.selectedTagSchema = 'MAP';
		component.selectedTagLevel = TagLevel.Country;
		component.selectedValueTagType = ValueTagType.Exclude;
		component.selectedTagCountryId = 'US';
		component.selectedTagElement = new SchemaElement();
		component.selectedTagElement.Id = 'tag element id';
		component.selectedTagValueId = 'tag value id';
		const valueTag: ValueTag = component.createValueTag(valueId);
		expect(valueTag.ValueId).toEqual(valueId);
		expect(valueTag.TagElementId).toBeUndefined();
		expect(valueTag.TagValueId).toBeUndefined();
		expect(valueTag.CountryId).toBe('US');
		expect(valueTag.SchemaTag).toBeUndefined();
		expect(valueTag.Type).toEqual(ValueTagType.Exclude, 'ValueTagType should be exclude');
	});
});
