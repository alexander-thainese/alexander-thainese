import { Component, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';

import { environment } from '../../../environments/environment';
import { MessageService } from '../message/message.service';
import { Message } from '../message/message';

@Component({
	selector: 'file-upload',
	template: `
    <div>
        <div (dragenter)="onDragEnter($event)" (dragover)="onDragOver($event)" (dragleave)="onDragLeave($event)" (drop)="onDrop($event)">
            <mat-progress-spinner mode="determinate" [value]="progress" color="primary" style="width: 300px; height: 300px; margin-left: -24px;"></mat-progress-spinner>
            <div style="z-index: 201;" id="test" (dragenter)="onDragEnter($event)" (dragover)="onDragOver($event)" (dragleave)="onDragLeave($event)" (drop)="onDrop($event)">
                <div *ngIf="!uploadedFile" style="z-index: 200; margin-top: -284px;">
                  <img src="/assets/images/upload.png" />
                </div>
                <div *ngIf="uploadedFile" class="circle" style="z-index: 200; margin-top: -284px;">
                    <span>{{uploadedFile}}</span>
                </div>
            </div>
        </div>
        <div style="width: 100%; text-align: center; margin-top: 30px;">
            <button type="button" mat-raised-button (click)="choose(fileInput)" style="width: 150px; margin-right: 5px; margin-left: -20px;">SELECT FILE
                <input #fileInput type="file" (change)="fileChangeEvent($event)" placeholder="Upload file..." style="display: none;" />
            </button>
            <button type="button" mat-raised-button style="width: 150px; margin-left: 5px;">SELECT FROM S3</button>
            
        </div>
    </div>`,
	styles: [
		`
    .circle {
        width: 263px; 
        height: 263px;
        background: #F7F7F7; 
        -moz-border-radius: 131.5px; 
        -webkit-border-radius: 131.5px; 
        border-radius: 131.5px;
        display: table;
        padding: 25px;
        text-align: center;
    }

    .circle span
    {
        max-width: 200px;
        word-wrap: break-word;
        display:table-cell;
        vertical-align:middle;
    }
    `
	]
})
export class FileUploadComponent implements OnInit {
	@Output() uploadComplete: EventEmitter<any> = new EventEmitter();
	filesToUpload: Array<File>;

	progress = 0;
	uploadedFile: string;

	constructor(private messageService: MessageService) {
		this.filesToUpload = [];
	}

	ngOnInit() {}

	fileChangeEvent(event: any) {
		this.filesToUpload = event.dataTransfer ? event.dataTransfer.files : event.target.files;
		this.upload();
	}

	upload() {
		this.makeFileRequest(environment.baseUrl + 'api/import/upload', [], this.filesToUpload).then(
			(result) => {},
			(error) => {
				this.progress = 0;
				this.messageService.showMessage({ severity: 'error', summary: JSON.parse(error).Errors.join('\n') });
			}
		);
	}

	choose(fileInput) {
		fileInput.value = null;
		fileInput.click();
	}

	getSelectedFile() {
		if (this.filesToUpload == null || this.filesToUpload.length == 0) return 'No file selected';

		return this.filesToUpload[0].name;
	}

	makeFileRequest(url: string, params: Array<string>, files: Array<File>) {
		return new Promise((resolve, reject) => {
			var formData: any = new FormData();
			var xhr = new XMLHttpRequest();

			formData.append('uploads[]', files[0], files[0].name);

			xhr.upload.addEventListener(
				'progress',
				(e: ProgressEvent) => {
					if (e.lengthComputable) {
						this.progress = Math.round(e.loaded * 100 / e.total);
					}
				},
				false
			);

			xhr.onreadystatechange = () => {
				if (xhr.readyState == 4) {
					if (xhr.status == 201) {
						this.uploadComplete.emit({
							fileName: files[0].name,
							fileUniqueName: JSON.parse(xhr.response).Result
						});
						this.uploadedFile = files[0].name;
					} else {
						reject(xhr.response);
					}
				}
			};
			xhr.open('POST', url, true);
			xhr.send(formData);
		});
	}

	onDragEnter(e) {
		e.stopPropagation();
		e.preventDefault();
	}

	onDragOver(e) {
		e.stopPropagation();
		e.preventDefault();
	}

	onDragLeave(e) {}

	onDrop(e) {
		e.stopPropagation();
		e.preventDefault();
		this.fileChangeEvent(e);
	}
}
