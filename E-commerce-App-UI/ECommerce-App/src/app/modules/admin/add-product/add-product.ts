import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Service } from '../service';
import { environment } from '../../../Environment/environment';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-product',
  standalone: false,
  templateUrl: './add-product.html',
  styleUrl: './add-product.css'
})
export class AddProduct {
  productForm!: FormGroup;
  selectedFiles: File[] = [];
  selectedImages: string[] = [];
  isEditMode = false;
  editId: number | null = null;

  categories: any[] = [];
  subCategories: any[] = [];
  suppliers: any[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly http: HttpClient,
    private readonly adminService: Service,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      price: ['', Validators.required],
      stockQuantity: ['', Validators.required],
      sku: ['', Validators.required],
      categoryId: ['', Validators.required],
      subCategoryId: ['', Validators.required],
      supplierId: ['', [Validators.required]]
    });

    this.loadSuppliers();
    this.loadCategory();

    const editData = history.state?.editData;
    if (editData) {
      this.isEditMode = true;
      this.editId = editData.id;
      this.productForm.patchValue({
        name: editData.name,
        description: editData.description,
        price: editData.price,
        stockQuantity: editData.stockQuantity,
        sku: editData.sku,
        supplierId: editData.supplierId
      });
      if (editData.categoryId) {
        this.adminService.getSubCategoriesByCategory(editData.categoryId).subscribe({
          next: (res) => {
            this.subCategories = res.response || [];
            this.productForm.patchValue({
              categoryId: editData.categoryId,
              subCategoryId: editData.subCategoryId
            });
          }
        });
      }
      if (editData.imageUrls?.length) {
        this.selectedImages = editData.imageUrls.map((url: string) => `${environment.assetUrl}${url}`);
      }
    }
  }


    onFileSelected(event: any): void {
      const files = Array.from(event.target.files) as File[];

      if (!files.length) return;

      // ✅ Add newly selected files (don’t reset existing ones)
      this.selectedFiles.push(...files);

      // ✅ Add instant previews for new files
      files.forEach((file) => {
        const objectURL = URL.createObjectURL(file);
        this.selectedImages.push(objectURL);
      });

      // ✅ Clear input to allow re-selecting same files later if needed
      event.target.value = '';
    }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      this.toastr.warning('Please fill all required fields.');
      return;
    }

    if (this.isEditMode && this.editId) {
      const updatePayload = {
        id: this.editId,
        name: this.productForm.get('name')?.value,
        description: this.productForm.get('description')?.value,
        price: this.productForm.get('price')?.value,
        stockQuantity: this.productForm.get('stockQuantity')?.value,
        sku: this.productForm.get('sku')?.value,
        subCategoryId: this.productForm.get('subCategoryId')?.value,
        supplierId: this.productForm.get('supplierId')?.value
      };
      this.adminService.updateProduct(updatePayload).subscribe({
        next: () => {
          this.toastr.success('Product updated successfully');
          this.router.navigate(['/admin/allproduct']);
        },
        error: (err) => this.toastr.error(err?.error?.message || 'Update failed')
      });
    } else {
      const supplierId = localStorage.getItem('supplierId');
      const formData = new FormData();
      formData.append('Name', this.productForm.get('name')?.value);
      formData.append('Description', this.productForm.get('description')?.value);
      formData.append('Price', this.productForm.get('price')?.value);
      formData.append('StockQuantity', this.productForm.get('stockQuantity')?.value);
      formData.append('SKU', this.productForm.get('sku')?.value);
      formData.append('SubCategoryId', this.productForm.get('subCategoryId')?.value);
      formData.append('SupplierId', supplierId || this.productForm.get('supplierId')?.value);
      this.selectedFiles.forEach(file => {
        formData.append('Images', file, file.name);
      });

      this.adminService.addProduct(formData).subscribe({
        next: () => {
          this.toastr.success('Product added successfully');
          this.productForm.reset();
          this.selectedImages = [];
          this.selectedFiles = [];
        },
        error: (err) => this.toastr.error(err?.error?.message || 'Error adding product')
      });
    }
  }


  removeImage(index: number): void {
    if (this.selectedImages[index]?.startsWith('blob:')) {
      URL.revokeObjectURL(this.selectedImages[index]);
    }
    this.selectedFiles.splice(index, 1);
    this.selectedImages.splice(index, 1);
  }
  
  loadSuppliers() {
    this.adminService.getAllSuppliers().subscribe({
      next: (res) => {
        if (res && Array.isArray(res.response)) {
          this.suppliers = res.response;
        } else {
          this.suppliers = [];
        }
      },
      error: (err) => console.error('Failed to load suppliers:', err)
    });
  }

  loadCategory() {
    this.adminService.getAllCategory().subscribe({
      next: (res) => {
        this.categories = res.response || [];
      }
    });
  }

  onCategoryChange(event: any) {
    const categoryId = this.productForm.get('categoryId')?.value;
    if (!categoryId) return;
    this.productForm.patchValue({ subCategoryId: '' });
    this.adminService.getSubCategoriesByCategory(categoryId).subscribe({
      next: (res) => {
        this.subCategories = res.response || [];
      }
    });
  }
}
