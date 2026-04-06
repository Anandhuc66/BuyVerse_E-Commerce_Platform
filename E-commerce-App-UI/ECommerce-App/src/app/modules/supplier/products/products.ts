import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../Environment/environment';

@Component({
  selector: 'app-products',
  standalone: false,
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class Products implements OnInit {
public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
    rowData: any[] = [];
    loading = true;
    allRowData: any[] = [];
    searchText = '';
    selectedCategory = '';
    selectedStatus = '';
    categories: string[] = [];
    statuses: string[] = [];
    private gridApi!: GridApi;

columnDefs: ColDef[] = [
  { headerName: 'Product Id', field: 'id', width: 100, cellStyle: { textAlign: 'right' } },

  { headerName: 'Name', field: 'name', width: 120, cellStyle: { textAlign: 'left' } },

  {
    headerName: 'Image',
    field: 'imageUrls',
    width: 120,
    cellStyle: { textAlign: 'center' },
    cellRenderer: (params: any) => {
      const imageUrl = params.value?.length
        ? `${environment.assetUrl}${params.value[0]}`
        : 'assets/no-image.png';
      return `
        <div style="display:flex; align-items:center; justify-content:center;">
          <img src="${imageUrl}" alt="product" width="50" height="50"
               style="border-radius:6px; object-fit:cover;" />
        </div>`;
    }
  },

  { headerName: 'Description', field: 'description', width: 180, cellStyle: { textAlign: 'left' } },

  { field: 'price', headerName: 'Price (â‚¹)', width: 100, cellStyle: { textAlign: 'right' } },

  { field: 'sku', headerName: 'SKU', width: 120, cellStyle: { textAlign: 'left' } },

  { field: 'stockQuantity', headerName: 'Stock', width: 120, cellStyle: { textAlign: 'right' } },

  { field: 'categoryName', headerName: 'Category', width: 120, cellStyle: { textAlign: 'left' } },


  {
    headerName: 'Created Date',
    field: 'createdAt',
    width: 120,
    cellStyle: { textAlign: 'center' },
    valueFormatter: (params) =>
      params.value ? new Date(params.value).toLocaleDateString('en-GB') : ''
  },

  {
    headerName: 'Actions',
    width: 130,
    cellStyle: { textAlign: 'center' },
    cellRenderer: () => `
      <div style="display:flex; justify-content:center; gap:8px;">
        <i class="bi bi-pencil" title="Edit" style="cursor:pointer; color:#198754;"></i>
        <i class="bi bi-trash" title="Delete" style="cursor:pointer; color:#dc3545;"></i>
      </div>
    `
  }
];

  // ];
  constructor(private readonly supplierService: Service, private readonly toastr: ToastrService, private readonly router: Router, private readonly cdr: ChangeDetectorRef) {}
  
    ngOnInit() {
      this.loadProducts();
    }
  
    onGridReady(params: any) {
      this.gridApi = params.api;
    }
  
loadProducts() {
  this.loading = true;
  this.supplierService.getProductByUserId().subscribe({
    next: (res) => {
      if (res?.response && Array.isArray(res.response.products)) {
        this.allRowData = [...res.response.products];
        this.rowData = [...this.allRowData];
        this.categories = [...new Set(this.allRowData.map((p: any) => p.categoryName).filter(Boolean))] as string[];
        this.statuses = [...new Set(this.allRowData.map((p: any) => p.status).filter(Boolean))] as string[];
      }
      this.loading = false;
      this.cdr.detectChanges();
    },
    error: (err) => {
      console.error('Error fetching products:', err);
      this.loading = false;
      this.cdr.detectChanges();
    }
  });
}

  onCellClicked(event: any): void {
    const target = event.event?.target as HTMLElement;
    if (!target) return;
    const data = event.data;

    if (target.classList.contains('bi-pencil')) {
      this.router.navigate(['/supplier/product-add'], { state: { editData: data } });
    }

    if (target.classList.contains('bi-trash')) {
      if (confirm(`Delete product "${data.name}"?`)) {
        this.supplierService.deleteProduct(data.id).subscribe({
          next: () => { this.toastr.success('Product deleted'); this.loadProducts(); },
          error: (err: any) => this.toastr.error(err?.error?.message || 'Delete failed')
        });
      }
    }
  }

  applyFilters() {
    let filtered = [...this.allRowData];
    if (this.searchText) {
      const s = this.searchText.toLowerCase();
      filtered = filtered.filter(r => r.name?.toLowerCase().includes(s) || r.description?.toLowerCase().includes(s));
    }
    if (this.selectedCategory) {
      filtered = filtered.filter(r => r.categoryName === this.selectedCategory);
    }
    if (this.selectedStatus) {
      filtered = filtered.filter(r => r.status === this.selectedStatus);
    }
    this.rowData = filtered;
    this.cdr.detectChanges();
  }

}
