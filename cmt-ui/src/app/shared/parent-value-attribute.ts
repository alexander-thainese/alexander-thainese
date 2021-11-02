export class ParentValueAttribute {
	ParentId: string;
	ElementName: string;
	AttributeValue: string;
	ElementType: AttributeElementType;
	Color: string;
}

export enum AttributeElementType {
	BU = 8,
	SUBBU = 16,
	PORTFOLIO = 32,
	INDICATION = 64
}
