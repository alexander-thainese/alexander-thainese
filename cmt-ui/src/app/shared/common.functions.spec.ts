import { checkFlag } from './common.functions';

describe('common functions', () => {
	it('check flag should return', () => {
		expect(checkFlag(2, 1)).toBeFalsy();
		expect(checkFlag(3, 1)).toBeTruthy();
		expect(checkFlag(5, 4)).toBeTruthy();
		expect(checkFlag(6, 2)).toBeTruthy();
	});
});
