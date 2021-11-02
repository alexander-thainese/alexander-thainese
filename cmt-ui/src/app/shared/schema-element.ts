import { ParentValueAttribute } from './parent-value-attribute';

export class SchemaElement {
	Id: string;
	RootId: string;
	Name: string;
	GlobalCode: string;
	Type: ItemType;
	AllValues: number;
	LocalValues: number;
	Children: SchemaElement[];
	ParentId: string;
	Channel: string;
	Status: boolean;
	HasTags: boolean;
	HasChildren: boolean;
	LocalValue: string;
	LocalCode: string;
	AllowChildren = true;
	Level: number;
	IsLov: boolean;
	LevelName: string;
	DataType: string;
	SearchTermFound: boolean;
	Readonly: boolean;
	DefinedBy: string;
	DefinitionDate: Date;
	ActivatedBy: string;
	ActivationDate: Date;
	DeactivatedBy: string;
	DeactivationDate: Date;
	TypeId: string;
	Attributes: number;
	SourceElementId: string;
	DefaultValue?: ListValue;
	ParentValueId: string;
	Description?: string;
	IsSelected?: boolean;
	ParentValuesAttributes: ParentValueAttribute[];

	constructor() {}
}

export enum ItemType {
	Schema = 1,
	Element,
	Item,
	Group
}

export class ListValue {
	public Name?: string;
	public Value?: string;

	constructor(name?: string, value?: string) {
		this.Name = name;
		this.Value = value;
	}
}
