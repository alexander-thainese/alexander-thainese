import { Component, OnInit } from '@angular/core';
import { HttpClient } from '../shared/http-client.service';
import { Router } from '@angular/router';
import { FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';

@Component({
	selector: 'app-login',
	templateUrl: './login.component.html',
	styleUrls: [ './login.component.scss' ]
})
export class LoginComponent implements OnInit {
	get username(): AbstractControl {
		return this.loginForm.get('username');
	}
	get password(): AbstractControl {
		return this.loginForm.get('password');
	}
	showSpinner = false;
	loginForm: FormGroup;

	constructor(private readonly httpClient: HttpClient, private readonly router: Router) {}

	ngOnInit() {
		if (this.httpClient.isTokenValid()) {
			this.router.navigate([ '/' ]);
		}
		this.loginForm = new FormGroup({
			username: new FormControl('', [ Validators.required ]),
			password: new FormControl('', [ Validators.required ])
		});
	}

	login() {
		this.username.markAsTouched();
		this.password.markAsTouched();
		this.loginForm.updateValueAndValidity({ onlySelf: false });
		if (!this.username.value || !this.password.value) {
			return;
		}
		this.showSpinner = true;

		this.httpClient.authenticateWithPassword(this.username.value, this.password.value).subscribe(
			(loginResult: boolean) => {
				this.showSpinner = false;
				if (loginResult === true) {
					this.router.navigate([ '/' ]);
				} else {
					alert('Combination of username and password is incorrect');
				}
			},
			(error: any) => {
				this.showSpinner = false;
			}
		);
	}
}
