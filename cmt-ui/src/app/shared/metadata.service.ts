import { Injectable } from '@angular/core';
import { Headers, Response, RequestOptions, ResponseContentType } from '@angular/http';
import { Observable, of, pipe } from 'rxjs';
import { SchemaElement, ItemType } from './schema-element';
import { environment } from '../../environments/environment';
import { MessageService } from './message/message.service';
import { HttpClient } from './http-client.service';
import { ExternalSystem } from './tree-view/element-detail/metadata-element-detail/external-system';

import { saveAs } from 'file-saver';
import { ParentValueAttribute } from './parent-value-attribute';
import { MarketService } from './market/market.service';
import { map, catchError, tap } from 'rxjs/operators';
import { ValueTag } from '../metadata-elements/manage-element-tags/manage-element-tags.component';
import { Country } from './country.model';

@Injectable()
export class MetadataService {
	private baseUrl = environment.baseUrl;
	private getMetadataElementsUrl: string = environment.baseUrl +
		'api/AdminMetadata/GetMetadataElementsTree/' +
		this.marketService.marketCode;
	private getSchemasUrl: string = environment.baseUrl +
		'api/AdminSchema/GetSchemaTree/' +
		this.marketService.marketCode;
	private getSchemaListUrl: string = environment.baseUrl +
		'api/AdminSchema/GetSchemas/' +
		this.marketService.marketCode;
	private getSchemaDetailsUrl = 'api/AdminMetadata/GetSchemaDetails/';
	private getValueTranslationsUrl: string = environment.baseUrl + 'api/AdminMetadata/GetValueTranslations/';
	private addChildValueUrl: string = environment.baseUrl + 'api/AdminMetadata/AddChildValue';
	private addElementUrl: string = environment.baseUrl + 'api/AdminMetadata/AddElement';
	private getThirdPartySystemsAccessBySchemaId: string = environment.baseUrl +
		'api/AdminSchema/GetThirdPartySystemsAccessBySchemaId/';
	private getThirdPartySystemsAccessByElementId: string = environment.baseUrl +
		'api/AdminMetadata/GetThirdPartySystemsAccessByElementId/';
	private updateSchemaThirdPartySystemsAccess: string = environment.baseUrl +
		'api/AdminSchema/UpdateThirdPartySystemsAccess';
	private updateElementThirdPartySystemsAccess: string = environment.baseUrl +
		'api/AdminMetadata/UpdateThirdPartySystemsAccess';
	private getElementDefinition: string = environment.baseUrl + 'api/AdminMetadata/GetElementDefinition/';
	private getChannelsUrl: string = environment.baseUrl + 'api/AdminSchema/GetChannels';
	private getCreateSchemaUrl: string = environment.baseUrl + 'api/AdminSchema/CreateSchema';
	private getElementDetailsUrl: string;
	private getValueDetailsUrl: string;
	private updateSchemaDescriptionUrl: string = environment.baseUrl + 'api/AdminSchema/UpdateSchemaDescription';
	private getParentsValueAttributesUrl: string = environment.baseUrl + 'api/AdminMetadata/GetValuesAttributes';
	private addValueTagsUrl = environment.baseUrl + 'api/AdminMetadata/AddValueTags';
	private getValueTagsUrl = environment.baseUrl + 'api/AdminMetadata/GetValueTags/';
	private removeValueTagUrl = environment.baseUrl + 'api/AdminMetadata/RemoveValueTag/';
	private adminMetadataElementsUrl = environment.baseUrl + 'api/AdminMetadata/Elements/';

	constructor(
		private readonly http: HttpClient,
		private readonly messageService: MessageService,
		private readonly marketService: MarketService
	) {}

	getSchemaElementDetails(id: string, type: ItemType): Promise<void | SchemaElement> {
		let url = this.getObjectDetailsUrl(id, type);
		if (url == null) {
			return;
		} else {
			return this.http.get(url).toPromise().then((response) => response as SchemaElement).catch((error) => {
				this.handleError(error, this);
			});
		}
	}

	updateElementName(id: string, name: string): Promise<string> {
		const url = this.baseUrl + 'api/AdminMetadata/UpdateMetadataElementName';

		return this.http
			.post(url, {
				id: id,
				name: name
			})
			.toPromise()
			.then((response) => 'ok')
			.catch((error) => {
				this.handleError(error, this);
				return '';
			});
	}

	updateElementType(id: string, typeId: string) {
		const url = this.baseUrl + 'api/AdminMetadata/UpdateElementType';

		return this.http
			.post(url, {
				id,
				typeId
			})
			.pipe(
				tap((response: boolean) => {
					if (response !== true) {
						this.messageService.showMessage({
							severity: 'error',
							summary: 'An error occured. Element type was not saved.'
						});
					}
				}),
				catchError((error: any) => {
					this.handleError(error, this);
					return of({});
				})
			);
	}

	updateValueTextValue(id: string, name: string): Promise<string> {
		const url = this.baseUrl + 'api/AdminMetadata/UpdateValueTextValue';

		return this.http
			.post(url, {
				id: id,
				textValue: name
			})
			.toPromise()
			.then((response) => 'ok')
			.catch((error) => {
				this.handleError(error, this);
				return '';
			});
	}

	updateValueGlobalCode(id: string, name: string, code: string): Promise<string> {
		const url = this.baseUrl + 'api/AdminMetadata/UpdateValueTextValue';

		return this.http
			.post(url, {
				id: id,
				textValue: name,
				globalCode: code
			})
			.toPromise()
			.then((response) => 'ok')
			.catch((error) => {
				this.handleError(error, this);
				return '';
			});
	}

	getMetadataElementDetails(id: string) {
		let url = this.baseUrl + 'api/AdminMetadata/GetMetadataElementDetails/' + id;
		return this.http.get(url).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	updateMetadataElementDescription(id: string, description: string) {
		this.verifyPrivileges();
		let url = this.baseUrl + 'api/AdminMetadata/UpdateMetadataElementDescription';
		return this.http
			.post(url, { id: id, description: description })
			.toPromise()
			.then((response) => 'ok')
			.catch((error) => {
				this.handleError(error, this);
			});
	}

	getParentElement(id: string) {
		let url = this.baseUrl + 'api/AdminMetadata/GetParentElement/' + id;
		return this.http.get(url).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	getDependentElements(id: string) {
		let url = this.baseUrl + 'api/AdminMetadata/getDepentdentElements/' + id;
		return this.http.get(url).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	updateMetadataElementDetails(mdElement: any) {
		this.verifyPrivileges();
		let url = this.baseUrl + 'api/AdminMetadata/UpdateMetadataElementDetails';
		return this.http.post(url, mdElement).toPromise().then((response) => 'ok').catch((error) => {
			this.handleError(error, this);
		});
	}

	TranslateValue(translation: any) {
		let url = this.baseUrl + 'api/AdminMetadata/TranslateValue/' + this.marketService.marketCode;
		return this.http.post(url, translation).toPromise().then((response) => 'ok').catch((error) => {
			this.handleError(error, this);
			return '';
		});
	}
	getSchemaList(searchTerm: string): Promise<void | SchemaElement[]> {
		let url = this.getSchemaListUrl;
		if (searchTerm) {
			url += '/' + encodeURI(searchTerm);
		}
		return this.http.get(url).toPromise().then((response) => response as SchemaElement[]).catch((error) => {
			this.handleError(error, this);
		});
	}
	getSchemas(searchTerm: string): Promise<void | SchemaElement[]> {
		let url = this.getSchemasUrl + '/search/';
		if (searchTerm) {
			url += encodeURI(searchTerm);
		}
		return this.http.get(url).toPromise().then((response) => response as SchemaElement[]).catch((error) => {
			this.handleError(error, this);
		});
	}

	getSingleSchema(searchTerm: string, schemaid: string): Promise<void | SchemaElement[]> {
		let url = this.getSchemasUrl + '/' + schemaid;

		return this.http.get(url).toPromise().then((response) => response as SchemaElement[]).catch((error) => {
			this.handleError(error, this);
		});
	}

	getMetadataElements(searchTerm: string): Promise<void | SchemaElement[]> {
		let url = this.getMetadataElementsUrl;
		if (searchTerm) {
			url += '/' + encodeURI(searchTerm);
		}
		return this.http.get(url).toPromise().then((response) => response as SchemaElement[]).catch((error) => {
			this.handleError(error, this);
		});
	}

	getMetadataElementTypes(): Observable<any> {
		const url = this.baseUrl + 'api/AdminMetadata/GetElementTypes';
		return this.http.get(url);
	}

	requestValueDeletion(id: string): Promise<void | boolean> {
		let url = this.baseUrl + 'api/AdminMetadata/disableValue/' + id;
		return this.http.post(url, null).toPromise().then((response) => true).catch((error) => {
			this.handleError(error, this);
		});
	}

	getValueTranslations(id: string): Promise<void | string[]> {
		const url = this.getValueTranslationsUrl + id;
		return this.http.get(url).toPromise().then((response) => response as string[]).catch((error) => {
			this.handleError(error, this);
		});
	}
	addChildValue(element: SchemaElement): Promise<string> {
		const url = this.addChildValueUrl;
		return this.http.post(url, element).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	addElement(element: any): Promise<string> {
		const url = this.addElementUrl;
		return this.http.post(url, element).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	deleteElement(id: string, type: ItemType) {
		const url = this.baseUrl + 'api/AdminMetadata/DeleteObject';
		return this.http.post(url, { id: id, type: type }).toPromise().then((response) => 'ok').catch((error) => {
			this.handleError(error, this);
		});
	}

	removeDerivedElement(id: string) {
		const url = this.baseUrl + `api/AdminMetadata/RemoveDerivedElement/${id}`;
		return this.http.post(url, {}).toPromise().then((response) => 'ok').catch((error) => {
			this.handleError(error, this);
		});
	}

	activateElement(id: string, type: ItemType) {
		const url = this.baseUrl + 'api/AdminMetadata/ReactivateObject';
		return this.http.post(url, { id: id, type: type }).toPromise().then((response) => 'ok').catch((error) => {
			this.handleError(error, this);
		});
	}

	verifyPrivileges() {
		if (!this.http.isAdmin) {
			throw new Error('You are not authorized to perform this operation');
		}
	}

	getThirdPartySystemsAccess(id: string, schema: boolean) {
		let url: string;
		if (schema) {
			url = this.getThirdPartySystemsAccessBySchemaId + id;
		} else {
			url = this.getThirdPartySystemsAccessByElementId + id;
		}
		return this.http.get(url).toPromise().then((response) => response as ExternalSystem[]).catch((error) => {
			this.handleError(error, this);
		});
	}

	updateThirdPartySystemsAccess(system: ExternalSystem, schema: boolean) {
		let url: string;
		if (schema) {
			url = this.updateSchemaThirdPartySystemsAccess;
		} else {
			url = this.updateElementThirdPartySystemsAccess;
		}
		return this.http.post(url, system).toPromise().then((response) => response).catch((error) => {
			this.handleError(error, this);
		});
	}

	updateSchemaDescription(id: string, description: string) {
		this.verifyPrivileges();
		return this.http
			.post(this.updateSchemaDescriptionUrl, { id: id, description: description })
			.toPromise()
			.then((response) => 'ok')
			.catch((error) => {
				this.handleError(error, this);
			});
	}

	downloadElementDefinition(elementId: string, elementName: string): Promise<void | File> {
		const url = this.getElementDefinition + elementId + '/' + this.marketService.marketCode;

		let headers = new Headers({ 'Content-Type': 'application/json', Accept: 'application/txt' });
		let options = new RequestOptions({ headers: headers, responseType: ResponseContentType.Blob });
		let filename = this.marketService.marketCode + '_' + elementName.replace(/\s/g, '_') + '.txt';

		return this.http
			.get(url, options, true)
			.toPromise()
			.then((res) => {
				this.extractContent(res, filename);
			})
			.catch((error) => {
				this.handleError(error, this);
			});
	}

	getChannels(): Promise<void | any[]> {
		return this.http
			.get(this.getChannelsUrl)
			.toPromise()
			.then((res: any[]) => res)
			.catch((error) => this.handleError(error, this));
	}

	createSchema(schemaName: string, channelId: string, sourceSchemaId?: string): Promise<void | string> {
		return this.http
			.post(this.getCreateSchemaUrl, { Name: schemaName, ChannelId: channelId, SourceSchemaId: sourceSchemaId })
			.toPromise()
			.then((res: string) => res)
			.catch((error) => this.handleError(error, this));
	}

	getMetadataElementsWithDescription(id: string): Promise<void | SchemaElement[]> {
		const url = environment.baseUrl + 'api/AdminMetadata/GetActiveMetadataElementsWithDescription/' + id;
		return this.http
			.get(url)
			.toPromise()
			.then((res: SchemaElement[]) => res)
			.catch((error) => this.handleError(error, this));
	}

	updateSchemaElementSelection(id: string, elementIds: string[]) {
		let url = environment.baseUrl + 'api/AdminSchema/UpdateSchemaElements/' + id;
		return this.http.post(url, elementIds).toPromise().catch((error) => this.handleError(error, this));
	}

	deleteSchema(id: string) {
		let url = environment.baseUrl + 'api/AdminSchema/DeleteSchema/' + id;
		return this.http.post(url, null).toPromise().catch((error) => this.handleError(error, this));
	}

	activateSchema(id: string) {
		let url = environment.baseUrl + 'api/AdminSchema/ActivateSchema/' + id;
		return this.http.post(url, null).toPromise().catch((error) => this.handleError(error, this));
	}

	deactivateSchema(id: string) {
		let url = environment.baseUrl + 'api/AdminSchema/DeactivateSchema/' + id;
		return this.http.post(url, null).toPromise().catch((error) => this.handleError(error, this));
	}

	deactivateElement(id: string) {
		let url = environment.baseUrl + 'api/AdminMetadata/DeleteObject';
		return this.http
			.post(url, {
				id: id,
				type: ItemType.Element
			})
			.toPromise()
			.catch((error) => this.handleError(error, this));
	}

	getAllCountriesObservable(): Observable<Country[]> {
		let url = environment.baseUrl + 'api/AdminSchema/GetCountries';
		return this.http.get(url);
	}
	getAllCountries(): Promise<any> {
		return this.getAllCountriesObservable()
			.toPromise()
			.then((res) => res)
			.catch((error) => this.handleError(error, this));
	}

	getSchemaCountries(schemaId: string): Promise<void | any[]> {
		let url = environment.baseUrl + 'api/AdminSchema/GetCountries/' + schemaId;
		return this.http.get(url).toPromise().then((res: any[]) => res).catch((error) => this.handleError(error, this));
	}

	addSchemaCountry(schemaId: string, countryId: string) {
		let url = environment.baseUrl + 'api/AdminSchema/AddCountry/' + schemaId + '/' + countryId;
		return this.http.post(url, null).toPromise().then((res) => res).catch((error) => this.handleError(error, this));
	}

	deleteSchemaCountry(schemaId: string, countryId: string) {
		let url = environment.baseUrl + 'api/AdminSchema/DeleteCountry/' + schemaId + '/' + countryId;
		return this.http.post(url, null).toPromise().then((res) => res).catch((error) => this.handleError(error, this));
	}

	addSchemaTag(schemaId: string, tagName: string) {
		let url = environment.baseUrl + 'api/AdminSchema/AddTag/' + schemaId;
		return this.http
			.post(url, '"' + tagName + '"')
			.toPromise()
			.then((res) => res)
			.catch((error) => this.handleError(error, this));
	}

	deleteSchemaTag(schemaId: string, tagId: string) {
		let url = environment.baseUrl + 'api/AdminSchema/DeleteTag/' + schemaId + '/' + tagId;
		return this.http.post(url, null).toPromise().then((res) => res).catch((error) => this.handleError(error, this));
	}

	getSchemaTags(schemaId?: string): Observable<void | any[]> {
		if (!schemaId) {
			schemaId = '';
		}
		let url = environment.baseUrl + 'api/AdminSchema/GetTags/' + schemaId;
		return this.http.get(url).pipe(
			catchError((error: any) => {
				this.handleError(error, this);
				return of({});
			})
		);
	}

	setDefaultValue(elementId: string, value: string) {
		let url = environment.baseUrl + 'api/AdminMetadata/SetDefaultValue/' + elementId;
		if (value) {
			value = '"' + value + '"';
		}
		return this.http
			.post(url, value)
			.toPromise()
			.then((res) => res)
			.catch((error) => this.handleError(error, this));
	}

	setSchemaElementProperties(schemaId: string, elementId: string, value: any) {
		let url = environment.baseUrl + 'api/AdminSchema/SetSchemaElementProperties/' + schemaId + '/' + elementId;
		return this.http
			.post(url, value)
			.toPromise()
			.then((res) => res)
			.catch((error) => this.handleError(error, this));
	}

	addElementToSchema(schemaId: string, elementId: string) {
		let url = environment.baseUrl + 'api/AdminSchema/AddElementToSchema/' + schemaId + '/' + elementId;
		return this.http.post(url, null).toPromise().then((res) => res).catch((error) => this.handleError(error, this));
	}

	deleteElementFromSchema(schemaId: string, elementId: string) {
		let url = environment.baseUrl + 'api/AdminSchema/DeleteElementFromSchema/' + schemaId + '/' + elementId;
		return this.http.post(url, null).toPromise().then((res) => res).catch((error) => this.handleError(error, this));
	}

	getElementValuesForDefault(elementId): Promise<void | any[]> {
		let url = environment.baseUrl + 'api/AdminMetadata/GetElementValuesForDefault/' + elementId;
		return this.http
			.get(url, null)
			.toPromise()
			.then((res: any[]) => res)
			.catch((error) => this.handleError(error, this));
	}

	getAllParentValuesAttribute(): Promise<void | ParentValueAttribute[]> {
		return this.http
			.get(this.getParentsValueAttributesUrl)
			.toPromise()
			.then((response) => response as ParentValueAttribute[])
			.catch((error) => {
				this.handleError(error, this);
			});
	}

	addValueTags(valueTags: ValueTag[]): Observable<boolean> {
		return this.http.post(this.addValueTagsUrl, valueTags).pipe(
			map((response: boolean) => {
				if (response !== true) {
					this.messageService.showMessage({
						severity: 'error',
						summary: 'An error occured. Value Tags were not saved.'
					});
				}
				return response;
			}),
			catchError((error: any) => {
				this.handleError(error, this);
				return of(false);
			})
		);
	}

	getValueTags(valueId: string): Observable<ValueTag> {
		return this.http.get(this.getValueTagsUrl + valueId).pipe(
			catchError((error: any) => {
				this.handleError(error, this);
				return of(false);
			})
		);
	}

	removeValueTag(id: string): Observable<void> {
		return this.http.post(this.removeValueTagUrl + id, {}).pipe(
			catchError((error: any) => {
				this.handleError(error, this);
				return of(false);
			})
		);
	}

	handleError(error: any, source: any) {
		source.messageService.showMessage({ severity: 'error', summary: error.json().Message });
	}

	getCanDeleteDependentElement(elementId: string): Observable<boolean> {
		return this.http.get(this.adminMetadataElementsUrl + elementId + '/CanDelete').pipe(
			catchError((error: any) => {
				this.handleError(error, this);
				return of(false);
			})
		);
	}

	private getObjectDetailsUrl(id: string, type: ItemType) {
		let url: string;
		switch (type) {
			case ItemType.Schema:
				url = this.getSchemaDetailsUrl;
				break;
			case ItemType.Element:
				url = this.getElementDetailsUrl;
				break;
			default:
				this.handleError('Item type is out of range', this);
				return null;
		}
		return this.baseUrl + url + id;
	}

	private extractContent(res: Response, filename: string) {
		let blob: Blob = res.blob();
		saveAs(blob, filename);
	}
}
