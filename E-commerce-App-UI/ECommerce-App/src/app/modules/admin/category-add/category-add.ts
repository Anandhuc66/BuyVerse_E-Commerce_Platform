import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-category-add',
  standalone: false,
  templateUrl: './category-add.html',
  styleUrl: './category-add.css'
})
export class CategoryAdd {
  categoryForm!: FormGroup;
  isEditMode = false;
  editId: number | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly adminService: Service,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]]
    });

    const editData = history.state?.editData;
    if (editData) {
      this.isEditMode = true;
      this.editId = editData.id;
      this.categoryForm.patchValue({
        name: editData.name,
        description: editData.description
      });
    }
  }

  onSubmit(): void {
    if (this.categoryForm.valid) {
      const formData = this.categoryForm.value;

      if (this.isEditMode && this.editId) {
        this.adminService.updateCategory(this.editId, formData).subscribe({
          next: () => {
            this.toastr.success('Category updated successfully');
            this.router.navigate(['/admin/category']);
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Update failed')
        });
      } else {
        this.adminService.addCategory(formData).subscribe({
          next: () => {
            this.toastr.success('Category added successfully');
            this.categoryForm.reset();
          },
          error: (err) => this.toastr.error(err?.error?.message || 'Add failed')
        });
      }
    } else {
      this.categoryForm.markAllAsTouched();
    }
  }
}
