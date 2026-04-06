import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { trigger, transition, style, animate } from '@angular/animations';

interface UserAddress {
  id: number;
  fullAddress: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.css',
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(15px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class Profile implements OnInit {
  // User info from localStorage
  fullName = '';
  email = '';
  gender = '';
  phoneNumber = '';
  role = '';

  // Addresses
  addresses: UserAddress[] = [];
  addressesLoading = true;
  showAddressForm = false;
  addressForm!: FormGroup;

  // Change Password
  showPasswordForm = false;
  passwordForm!: FormGroup;

  // Active tab
  activeTab: 'info' | 'addresses' | 'security' = 'info';

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: Service,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {
    // Load user info from localStorage
    this.fullName = localStorage.getItem('fullName') || 'N/A';
    this.email = localStorage.getItem('email') || 'N/A';
    this.gender = localStorage.getItem('gender') || 'N/A';
    this.phoneNumber = localStorage.getItem('phoneNumber') || 'N/A';
    this.role = localStorage.getItem('role') || 'User';

    // Init forms
    this.addressForm = this.fb.group({
      fullAddress: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', Validators.required],
      country: ['India', Validators.required]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });

    // Load addresses
    this.loadAddresses();
  }

  loadAddresses() {
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    this.addressesLoading = true;
    this.userService.getUserAddresses(userId).subscribe({
      next: (res: any) => {
        this.addresses = res.response || [];
        this.addressesLoading = false;
      },
      error: () => {
        this.addresses = [];
        this.addressesLoading = false;
      }
    });
  }

  addAddress() {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    const userId = localStorage.getItem('userId')!;
    const body = { userId, ...this.addressForm.value };

    this.userService.addUserAddress(body).subscribe({
      next: () => {
        this.toastr.success('Address added!');
        this.addressForm.reset({ country: 'India' });
        this.showAddressForm = false;
        this.loadAddresses();
      },
      error: () => this.toastr.error('Failed to add address')
    });
  }

  changePassword() {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.value;

    if (newPassword !== confirmPassword) {
      this.toastr.error('Passwords do not match!');
      return;
    }

    this.userService.changePassword({ currentPassword, newPassword }).subscribe({
      next: (res: any) => {
        if (res?.isError) {
          this.toastr.error(res.errors?.[0]?.errorMessage || 'Failed');
          return;
        }
        this.toastr.success('Password changed successfully!');
        this.passwordForm.reset();
        this.showPasswordForm = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.errors?.[0]?.errorMessage || 'Failed to change password');
      }
    });
  }

  getInitials(): string {
    if (!this.fullName || this.fullName === 'N/A') return '?';
    return this.fullName
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  trackByAddress(index: number, addr: any): number {
    return addr.id;
  }
}
