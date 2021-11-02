export class RowClickEvent {
    originalEvent: any;
    row: any;

    constructor(originalEvent: any, row: any) {
        this.originalEvent = originalEvent;
        this.row = row;
    }
}