import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-supplier-add',
  standalone: false,
  templateUrl: './supplier-add.html',
  styleUrl: './supplier-add.css'
})
export class SupplierAdd {
  supplierForm!: FormGroup;
  isEditMode = false;
  editId: number | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly adminService: Service,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.supplierForm = this.fb.group({
      companyName: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      fullName: ['', Validators.required],
      gender: ['', Validators.required],
      password: ['', Validators.required],
    });

    const editData = history.state?.editData;
    if (editData) {
      this.isEditMode = true;
      this.editId = editData.id;
      this.supplierForm.patchValue({
        companyName: editData.companyName,
        contactEmail: editData.contactEmail,
        phone: editData.phone,
        fullName: editData.fullName || '',
        gender: editData.gender || ''
      });
      // In edit mode, password is not required
      this.supplierForm.get('password')?.clearValidators();
      this.supplierForm.get('password')?.updateValueAndValidity();
    }
  }

  onSubmit() {
    if (this.supplierForm.valid) {
      if (this.isEditMode && this.editId) {
        const updatePayload = {
          id: this.editId,
          companyName: this.supplierForm.value.companyName,
          contactEmail: this.supplierForm.value.contactEmail,
          phone: this.supplierForm.value.phone
        };
        this.adminService.updateSupplier(updatePayload).subscribe({
          next: () => {
            this.toastr.success('Supplier updated successfully');
            this.router.navigate(['/admin/allsuppliers']);
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Update failed')
        });
      } else {
        this.adminService.addSupplier(this.supplierForm.value).subscribe({
          next: () => {
            this.toastr.success('Supplier added successfully');
            this.supplierForm.reset();
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Add failed')
        });
      }
    } else {
      this.supplierForm.markAllAsTouched();
    }
  }
}