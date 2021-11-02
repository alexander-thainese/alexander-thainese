export class TokenResponse {
	access_token: string;
	token_type: string;
	expires_in: number;
	userName: string;
	'.issued': Date;
	'.expires': Date;
	roles: string[];
	markets: string[];
	constructor() {}
}
