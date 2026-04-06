import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-role-add',
  standalone: false,
  templateUrl: './role-add.html',
  styleUrl: './role-add.css'
})
export class RoleAdd {
  rolesForm!: FormGroup;
  isEditMode = false;
  editId: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly adminService: Service,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.rolesForm = this.fb.group({
      displayName: ['', [Validators.required]],
      description: ['', [Validators.required]]
    });

    const editData = history.state?.editData;
    if (editData) {
      this.isEditMode = true;
      this.editId = editData.id;
      this.rolesForm.patchValue({
        displayName: editData.displayName,
        description: editData.description
      });
    }
  }

  onSubmit(): void {
    if (this.rolesForm.valid) {
      const formData = this.rolesForm.value;

      if (this.isEditMode && this.editId) {
        const updatePayload = {
          id: this.editId,
          name: formData.displayName,
          displayName: formData.displayName,
          description: formData.description
        };
        this.adminService.updateRole(updatePayload).subscribe({
          next: () => {
            this.toastr.success('Role updated successfully');
            this.router.navigate(['/admin/roles']);
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Update failed')
        });
      } else {
        this.adminService.addRole(formData).subscribe({
          next: () => {
            this.toastr.success('Role added successfully');
            this.rolesForm.reset();
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Add failed')
        });
      }
    } else {
      this.rolesForm.markAllAsTouched();
    }
  }
}