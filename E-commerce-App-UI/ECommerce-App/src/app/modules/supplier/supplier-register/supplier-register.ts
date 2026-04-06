import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/service/auth.service';

@Component({
  selector: 'app-supplier-register',
  standalone: false,
  templateUrl: './supplier-register.html',
  styleUrl: './supplier-register.css'
})
export class SupplierRegister implements OnInit {
  infoForm!: FormGroup;
  passwordForm!: FormGroup;
  showPasswordSection = false;
  errorMessage = '';
  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.infoForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      companyName: ['', Validators.required],
      gender: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
    });

    this.passwordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  goToPasswordSection(): void {
    if (this.infoForm.valid) {
      this.showPasswordSection = true;
    } else {
      this.infoForm.markAllAsTouched();
    }
  }

  backToInfo() {
    this.showPasswordSection = false;
  }

  navigateHome() {
    this.router.navigate(['/']);
  }

  onRegister(): void {
    this.errorMessage = '';
    const { password, confirmPassword } = this.passwordForm.value;

    if (password !== confirmPassword) {
      this.errorMessage = 'Passwords do not match!';
      return;
    }

    if (this.infoForm.valid && this.passwordForm.valid) {
      const payload = {
        companyName: this.infoForm.value.companyName,
        contactEmail: this.infoForm.value.contactEmail,
        phone: this.infoForm.value.phone,
        gender: this.infoForm.value.gender,
        password: password
      };

      this.loading = true;
      this.authService.registerSupplier(payload).subscribe({
        next: (res: any) => {
          this.loading = false;
          if (res?.isError) {
            this.errorMessage = res.errors?.[0]?.errorMessage || 'Registration failed';
            return;
          }
          alert('Supplier registration successful! Please login.');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.errors?.[0]?.errorMessage || 'Registration failed. Please try again.';
        }
      });
    }
  }
}
