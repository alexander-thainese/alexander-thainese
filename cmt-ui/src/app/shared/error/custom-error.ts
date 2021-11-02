export class CustomError extends Error {
	constructor(message?: string) {
		super(message);
		this.name = 'CustomError';
		this.message = message;
		this.stack = (<any>new Error()).stack;
	}
}
