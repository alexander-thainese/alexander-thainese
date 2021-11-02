import { HasAttributesPipe } from './has-attributes-pipe.pipe';

describe('HasAttributesPipe', () => {
	it('create an instance', () => {
		const pipe = new HasAttributesPipe();
		expect(pipe).toBeTruthy();
	});
});
