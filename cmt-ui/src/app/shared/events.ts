export class EventBase<T> {
	source: T;

	constructor(source: T) {
		this.source = source;
	}
}

export class SelectionChangedEvent<T> extends EventBase<T> {
	key: any;
	isSelected: boolean;

	constructor(source: T, key: any, isSelected: boolean) {
		super(source);
		this.key = key;
		this.isSelected = isSelected;
	}
}
