export class ItemState {
    constructor(id?: string, isSelected?: boolean, rootId?: string) {
        this.id = id;
        this.rootId = rootId ? rootId : id;
        this.isSelected = isSelected;
        this.isExpanded = isSelected;
    }
    id: string;
    rootId: string;
    isSelected: boolean;
    isExpanded: boolean;
    startChildIndex: number;
}