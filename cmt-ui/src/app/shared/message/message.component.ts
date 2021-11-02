import { Component, Input, AfterViewInit, DoCheck, OnDestroy, IterableDiffers, ElementRef } from '@angular/core';

import { Message } from './message';
import { MessageService } from './message.service';

const zindex = 1000;

@Component({
	selector: 'message',
	templateUrl: 'message.component.html',
	styleUrls: [ 'message.component.css' ]
})
export class MessageComponent implements AfterViewInit, DoCheck, OnDestroy {
	private differ: any;
	private stopDoCheckPropagation: boolean;
	private timeout: any;
	private container: any;
	@Input() messages: Message[] = [];
	@Input() life = 5000;
	@Input() autoclose = true;
	zIndex: number;

	constructor(private el: ElementRef, differs: IterableDiffers, private messageService: MessageService) {
		this.differ = differs.find([]).create(null);
		this.zIndex = zindex;
		messageService.message.subscribe((item) => {
			this.onMessage(item);
		});
		messageService.clearConnectionError.subscribe(() => {
			this.messages = this.messages.filter((m) => !m.isConnectionError);
		});
	}

	ngAfterViewInit() {
		this.container = this.el.nativeElement.children[0];
	}

	ngDoCheck() {
		let changes = this.differ.diff(this.messages);

		if (changes) {
			if (this.stopDoCheckPropagation) {
				this.stopDoCheckPropagation = false;
			} else if (this.messages && this.messages.length) {
				// this.zIndex = ++zindex;
				this.fadeIn(this.container, 250);

				if (this.autoclose) {
					if (this.timeout) {
						clearTimeout(this.timeout);
					}

					this.timeout = setTimeout(() => {
						this.removeAll();
					}, this.life);
				}
			}
		}
	}

	remove(msg: Message, msgel: any) {
		this.stopDoCheckPropagation = true;

		this.fadeOut(msgel, 250);

		setTimeout(() => {
			this.messages.splice(this.findMessageIndex(msg), 1);
		}, 250);
	}

	removeAll() {
		if (this.messages && this.messages.length) {
			this.stopDoCheckPropagation = true;

			this.fadeOut(this.container, 250);

			setTimeout(() => {
				this.messages.splice(0, this.messages.length);
			}, 250);
		}
	}

	findMessageIndex(msg: Message) {
		let index = -1;

		if (this.messages && this.messages.length) {
			for (let i = 0; i < this.messages.length; i++) {
				if (this.messages[i] == msg) {
					index = i;
					break;
				}
			}
		}

		return index;
	}

	ngOnDestroy() {
		if (this.autoclose) {
			clearTimeout(this.timeout);
		}
	}

	private onMessage(message: Message): void {
		if (this.messages.filter((p) => p.severity === 'error' && message.severity === 'error').length > 0) {
			this.messages = this.messages.filter((p) => p.severity !== 'error');
		}
		this.messages.push(message);
		this.ngDoCheck();
	}

	private fadeIn(element, duration: number): void {
		element.style.opacity = 0;

		let last = +new Date();
		let tick = function() {
			element.style.opacity = +element.style.opacity + (new Date().getTime() - last) / duration;
			last = +new Date();

			if (+element.style.opacity < 1) {
				(window.requestAnimationFrame !== undefined && requestAnimationFrame(tick)) || setTimeout(tick, 16);
			}
		};

		tick();
	}

	private fadeOut(element, ms) {
		let opacity = 1,
			interval = 50,
			duration = ms,
			gap = interval / duration;

		let fading = setInterval(() => {
			opacity = opacity - gap;
			element.style.opacity = opacity;

			if (opacity <= 0) {
				clearInterval(fading);
			}
		}, interval);
	}
}
