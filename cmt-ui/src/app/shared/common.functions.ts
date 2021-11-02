export function checkFlag(value: number, flag: number): boolean {
	// tslint:disable-next-line: no-bitwise
	return (value & flag) === flag;
}
