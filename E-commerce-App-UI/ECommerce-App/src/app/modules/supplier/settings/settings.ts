import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../Environment/environment';

@Component({
  selector: 'app-supplier-settings',
  standalone: false,
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class SupplierSettings implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  businessForm!: FormGroup;

  profileLoading = false;
  passwordLoading = false;
  businessLoading = false;

  profileMessage = '';
  profileError = '';
  passwordMessage = '';
  changeError = '';
  businessMessage = '';
  businessError = '';

  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  private readonly baseUrl = environment.baseUrl;

  constructor(
    private readonly fb: FormBuilder,
    private readonly http: HttpClient,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      fullName: [localStorage.getItem('fullName') || '', Validators.required],
      email: [{ value: localStorage.getItem('email') || '', disabled: true }],
      phoneNumber: [localStorage.getItem('phoneNumber') || '', Validators.required],
      gender: [localStorage.getItem('gender') || '', Validators.required]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });

    this.businessForm = this.fb.group({
      companyName: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required]
    });

    // Load supplier business info
    this.loadSupplierInfo();
  }

  loadSupplierInfo(): void {
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    this.http.get(`${this.baseUrl}Supplier/GetSupplierByUserId/${userId}`).subscribe({
      next: (res: any) => {
        const supplier = res?.response;
        if (supplier) {
          this.businessForm.patchValue({
            companyName: supplier.companyName || '',
            contactEmail: supplier.contactEmail || '',
            phone: supplier.phone || ''
          });
          this.cdr.markForCheck();
        }
      },
      error: () => {}
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;

    this.profileLoading = true;
    this.profileMessage = '';
    this.profileError = '';

    const payload = {
      fullName: this.profileForm.value.fullName,
      gender: this.profileForm.value.gender,
      phoneNumber: this.profileForm.value.phoneNumber
    };

    this.http.put(`${this.baseUrl}Auth/update-profile`, payload).subscribe({
      next: (res: any) => {
        this.profileLoading = false;
        this.profileMessage = res?.message || 'Profile updated successfully.';
        localStorage.setItem('fullName', payload.fullName);
        localStorage.setItem('gender', payload.gender);
        localStorage.setItem('phoneNumber', payload.phoneNumber);
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.profileLoading = false;
        this.profileError = err?.error?.errors?.[0]?.errorMessage || 'Failed to update profile.';
        this.cdr.markForCheck();
      }
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;

    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.value;

    if (newPassword !== confirmPassword) {
      this.changeError = 'New and confirm values do not match.';
      return;
    }

    this.passwordLoading = true;
    this.passwordMessage = '';
    this.changeError = '';

    this.http.post(`${this.baseUrl}Auth/change-password`, {
      currentPassword,
      newPassword
    }).subscribe({
      next: (res: any) => {
        this.passwordLoading = false;
        this.passwordMessage = res?.message || 'Password changed successfully.';
        this.passwordForm.reset();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.passwordLoading = false;
        this.changeError = err?.error?.errors?.[0]?.errorMessage || 'Failed to change password.';
        this.cdr.markForCheck();
      }
    });
  }

  saveBusinessInfo(): void {
    if (this.businessForm.invalid) return;

    this.businessLoading = true;
    this.businessMessage = '';
    this.businessError = '';

    const userId = localStorage.getItem('userId');
    const supplierId = localStorage.getItem('supplierId');

    const payload = {
      id: Number(supplierId),
      companyName: this.businessForm.value.companyName,
      contactEmail: this.businessForm.value.contactEmail,
      phone: this.businessForm.value.phone
    };

    this.http.put(`${this.baseUrl}Supplier/update/${userId}`, payload).subscribe({
      next: (res: any) => {
        this.businessLoading = false;
        this.businessMessage = res?.message || 'Business info updated successfully.';
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.businessLoading = false;
        this.businessError = err?.error?.errors?.[0]?.errorMessage || 'Failed to update business info.';
        this.cdr.markForCheck();
      }
    });
  }
}
