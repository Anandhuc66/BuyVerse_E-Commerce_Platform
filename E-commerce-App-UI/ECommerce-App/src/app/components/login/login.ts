import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../core/service/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
loginForm!: FormGroup;
  errorMessage: string = '';
  showPassword = false;

  constructor(private readonly fb: FormBuilder, private readonly auth: AuthService, private readonly router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
  if (this.loginForm.invalid) return;

  this.auth.allLogin(this.loginForm.value).subscribe({
    next: (res: any) => {
      const role = res.response.role;

      // ✅ No need to call saveToken(), because it's already handled in AuthService.allLogin()
      localStorage.setItem('role', role);

      // ✅ Redirect based on role
      switch (role.toLowerCase()) {
        case 'admin':
          this.router.navigate(['/admin/dashboard']);
          break;

        case 'supplier':
          this.router.navigate(['/supplier/dashboard']);
          break;

        case 'user':
          this.router.navigate(['/homepage']);
          break;

        default:
          this.errorMessage = 'Unknown user role';
      }
    },
    error: () => {
      this.errorMessage = 'Invalid email or password';
    }
  });
}

}
