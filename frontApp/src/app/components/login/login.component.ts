import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage = '';
  loggedInUser = '';
  loginTime: string = '';

  @Output() loginSuccess = new EventEmitter<void>();

  constructor(private authService: AuthService) {}

  login() {
	  // send user and password and validate if exists
	  this.authService.login(this.username, this.password).subscribe({
		next: (response: any) => {
		  console.log("Login exitoso", response);
		  // show info on a panel
		  this.loggedInUser = this.username;
		  this.loginTime = new Date().toLocaleTimeString();
		  this.loginSuccess.emit();
		},
		error: (err) => {
		  console.error("Error en login", err);
		  this.errorMessage = err.error?.message || "Error desconocido ❌";
		}
	  });
	}
}