import { Injectable, EventEmitter } from '@angular/core';

import { Message } from './message';

@Injectable()
export class MessageService {
	message: EventEmitter<Message> = new EventEmitter<Message>();
	clearConnectionError: EventEmitter<any> = new EventEmitter<any>();
	constructor() {}

	showMessage(message: Message) {
		this.message.emit(message);
	}

	sendClearConnectionError() {
		this.clearConnectionError.emit(null);
	}
}
