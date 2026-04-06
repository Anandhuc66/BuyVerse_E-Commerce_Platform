import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-category',
  standalone: false,
  templateUrl: './category.html',
  styleUrl: './category.css'
})
export class Category {
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
    rowData: any[] = [];
    loading = true;
    allRowData: any[] = [];
    searchText = '';
    filterDate = '';
    private gridApi!: GridApi;
   columnDefs: ColDef[] = [
  { headerName: 'Category Id', field: 'id', width: 150, cellStyle: { textAlign: 'right' } },
  { headerName: 'Category Name', field: 'name', flex: 1, cellStyle: { textAlign: 'left' } },
  
  { headerName: 'Created Date', field: 'createdAt', width: 180, cellStyle: { textAlign: 'center' }, valueFormatter: (params) => params.value ? new Date(params.value).toLocaleDateString('en-GB') : '' },
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
  },
  { headerName: 'Description', field: 'description', flex: 2, cellStyle: { textAlign: 'left' } }
];

constructor(private readonly adminService: Service, private readonly toastr: ToastrService, private readonly router: Router, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadCategory();
  }

  onGridReady(params: any) {
    this.gridApi = params.api;
  }
  loadCategory(){
    this.loading = true;
    this.adminService.getAllCategory().subscribe({
      next: (res) => {
      if (res?.response && Array.isArray(res.response)) {
        this.allRowData = [...res.response];
        this.rowData = [...this.allRowData];
      }
      this.loading = false;
      this.cdr.detectChanges();
    },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    })
  }

  onCellClicked(event: any): void {
    const target = event.event?.target as HTMLElement;
    if (!target) return;
    const data = event.data;

    if (target.classList.contains('bi-pencil')) {
      this.router.navigate(['/admin/category-add'], { state: { editData: data } });
    }

    if (target.classList.contains('bi-trash')) {
      if (confirm(`Delete category "${data.name}"?`)) {
        this.adminService.deleteCategory(data.id).subscribe({
          next: () => { this.toastr.success('Category deleted'); this.loadCategory(); },
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
    if (this.filterDate) {
      const d = new Date(this.filterDate);
      filtered = filtered.filter(r => r.createdAt && new Date(r.createdAt) >= d);
    }
    this.rowData = filtered;
    this.cdr.detectChanges();
  }

}
