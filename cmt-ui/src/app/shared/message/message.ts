export abstract class Message {
	severity?: string;
	summary?: string;
	detail?: string;
	isConnectionError?: boolean;
	autoClose?: boolean;
}
