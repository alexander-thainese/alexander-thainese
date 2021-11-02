import { Injectable } from '@angular/core';

import { MessageService } from '../message/message.service'
import { Message } from '../message/message'
import { CustomError } from './custom-error'

@Injectable()
export class ErrorHandlerService {
    constructor(private messageService: MessageService) {

    }

    logError(error: any): void {
        let originalError = this.findOriginalError(error);
        let originalStack = this.findOriginalStack(error);
        let context = this.findContext(error);

        console.error(`EXCEPTION: ${this.extractMessage(error)}`);

        if (originalError) {
            console.error(`ORIGINAL EXCEPTION: ${this.extractMessage(originalError)}`);
        }

        if (originalStack) {
            console.error('ORIGINAL STACKTRACE:');
            console.error(originalStack);
        }

        if (context) {
            console.error('ERROR CONTEXT:');
            console.error(context);
        }

        if (originalError && originalError instanceof CustomError) {
            this.messageService.showMessage({ severity: 'error', summary: `message for custom error` });
        }
        else if (error.status) {
            if ((error.status == 500 || error.status == 406) && (error.Errors && error.Errors.length > 0)) {
                this.messageService.showMessage({ severity: 'error', summary: error.Errors.join('\n') });
            }
            else {
                this.messageService.showMessage({ severity: 'error', summary: error.status + ': ' + error.statusText });
            }
        }
        else {
            this.messageService.showMessage({ severity: 'error', summary: `${this.extractMessage(error)}` });
        }
    }

    private extractMessage(error: any): string {
        return error instanceof Error ? error.message : error.toString();
    }

    private findContext(error: any): any {
        if (error) {
            return error.context ? error.context : this.findContext(error.originalError);
        }

        return null;
    }

    private findOriginalError(error: any): any {
        let e = error.originalError;

        while (e && e.originalError) {
            e = e.originalError;
        }

        return e;
    }

    private findOriginalStack(error: any): string {
        if (!(error instanceof Error))
            return null;

        let e: any = error;
        let stack: string = e.stack;

        while (e.originalError) {
            e = e.originalError;
            if (e instanceof Error && e.stack) {
                stack = e.stack;
            }
        }

        return stack;
    }
}