import { ErrorHandler, forwardRef, Injectable, Inject } from '@angular/core';

import { ErrorHandlerService } from './error-handler.service';

export abstract class ErrorHandlerOptions {
	rethrowError: boolean;
}

export const ERROR_HANDLER_OPTIONS: ErrorHandlerOptions = {
	rethrowError: true
};

@Injectable()
export class CustomErrorHandler implements ErrorHandler {
	private options: ErrorHandlerOptions;

	constructor(
		private errorHandlerService: ErrorHandlerService,
		@Inject(ERROR_HANDLER_OPTIONS) options: ErrorHandlerOptions
	) {
		this.options = options;
	}

	handleError(error: any): void {
		this.errorHandlerService.logError(error);

		if (this.options.rethrowError) {
			throw error;
		}
	}
}

export let ERROR_HANDLER_PROVIDERS = [
	{
		provide: ERROR_HANDLER_OPTIONS,
		useValue: ERROR_HANDLER_OPTIONS
	},
	{
		provide: ErrorHandler,
		useClass: CustomErrorHandler
	}
];
