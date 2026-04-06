import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Service } from '../service';
import { environment } from '../../../Environment/environment';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-product-add',
  standalone: false,
  templateUrl: './product-add.html',
  styleUrl: './product-add.css'
})
export class ProductAdd implements OnInit {
  productForm!: FormGroup;
  selectedFiles: File[] = [];
  selectedImages: string[] = [];
  isEditMode = false;
  editId: number | null = null;

  categories: any[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly http: HttpClient,
    private readonly supplierService: Service,
    private readonly toastr: ToastrService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      price: [0, Validators.required],
      stockQuantity: [0, Validators.required],
      sku: ['', Validators.required],
      categoryId: ['', Validators.required],
      supplierId: [{ value: '', disabled: true }, Validators.required]
    });

    this.setSupplierIdFromLocalStorage();
    this.loadCategories();

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
        categoryId: editData.categoryId || editData.subCategoryId || ''
      });
      if (editData.imageUrls?.length) {
        this.selectedImages = editData.imageUrls.map((url: string) => `${environment.assetUrl}${url}`);
      }
    }
  }

  private setSupplierIdFromLocalStorage(): void {
    const storedSupplierId = localStorage.getItem('supplierId');
    if (storedSupplierId) {
      this.productForm.patchValue({ supplierId: storedSupplierId });
    }
  }

  private loadCategories(): void {
    this.http.get<any>(`${environment.baseUrl}Category`).subscribe({
      next: (res) => {
        this.categories = res.response || [];
      },
      error: () => { this.categories = []; }
    });
  }

  onFileSelected(event: any): void {
    const files = Array.from(event.target.files) as File[];
    if (!files.length) return;
    this.selectedFiles.push(...files);
    files.forEach((file) => {
      const objectURL = URL.createObjectURL(file);
      this.selectedImages.push(objectURL);
    });
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
        subCategoryId: this.productForm.get('categoryId')?.value,
        supplierId: this.productForm.get('supplierId')?.value
      };
      this.supplierService.updateProduct(updatePayload).subscribe({
        next: () => {
          this.toastr.success('Product updated successfully');
          this.router.navigate(['/supplier/products']);
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
      formData.append('CategoryId', this.productForm.get('categoryId')?.value);
      formData.append('SupplierId', supplierId ?? '');
      this.selectedFiles.forEach((file) => {
        formData.append('Images', file, file.name);
      });

      this.supplierService.addProduct(formData).subscribe({
        next: () => {
          this.toastr.success('Product added successfully');
          this.productForm.reset();
          this.selectedImages = [];
          this.selectedFiles = [];
          this.setSupplierIdFromLocalStorage();
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
}
