import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/service/auth.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(30px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class Register {
  infoForm!: FormGroup;
  passwordForm!: FormGroup;
  showPasswordStep = false;
  errorMessage = '';
  successMessage = '';
  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.infoForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      gender: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]]
    });

    this.passwordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  goToPassword(): void {
    if (this.infoForm.valid) {
      this.showPasswordStep = true;
    } else {
      this.infoForm.markAllAsTouched();
    }
  }

  backToInfo(): void {
    this.showPasswordStep = false;
  }

  onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    const { password, confirmPassword } = this.passwordForm.value;
    if (password !== confirmPassword) {
      this.errorMessage = 'Passwords do not match!';
      return;
    }

    if (this.infoForm.valid && this.passwordForm.valid) {
      const formData = {
        ...this.infoForm.value,
        password
      };

      this.loading = true;
      this.authService.registerUser(formData).subscribe({
        next: (res: any) => {
          this.loading = false;
          if (res?.isError) {
            this.errorMessage = res.errors?.[0]?.errorMessage || 'Registration failed';
            return;
          }
          this.successMessage = 'Registration successful! Redirecting to login...';
          setTimeout(() => this.router.navigate(['/login']), 2000);
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.errors?.[0]?.errorMessage || 'Registration failed. Please try again.';
        }
      });
    }
  }
}
