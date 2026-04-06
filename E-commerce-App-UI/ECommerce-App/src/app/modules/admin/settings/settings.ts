import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../Environment/environment';

@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  profileLoading = false;
  passwordLoading = false;

  profileMessage = '';
  profileError = '';
  passwordMessage = '';
  changeError = '';

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
        // Update localStorage
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
}
