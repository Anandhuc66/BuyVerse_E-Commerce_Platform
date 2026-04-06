import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-suppliers',
  standalone: false,
  templateUrl: './suppliers.html',
  styleUrl: './suppliers.css'
})
export class Suppliers {

  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
    rowData: any[] = [];
  allRowData: any[] = [];
  searchText = '';
  selectedStatus = '';
  statuses: string[] = [];
  loading = true;
  private gridApi!: GridApi;
  
  columnDefs: ColDef[] = [
    {headerName:'Supplier Id', field:'id', width: 120, cellStyle: { textAlign: 'right' } },
    { headerName: 'Company Name', field: 'companyName', sortable: true, filter: true, flex: 1, cellStyle: { textAlign: 'left' } },
    { headerName: 'Email', field: 'contactEmail', sortable: true, filter: true, flex: 1, cellStyle: { textAlign: 'left' } },
    { headerName: 'Phone', field: 'phone', sortable: true, filter: true, width: 150, cellStyle: { textAlign: 'left' } },
    {
      headerName: 'Status',
      field: 'status',
      width: 130,
      cellStyle: { textAlign: 'center' },
      cellRenderer: (params: any) => {
        const color = params.value === 'Verified' ? '#198754' : '#ffc107';
        return `<span style="color:${color}; font-weight:500;">${params.value}</span>`;
      }
    },
    {
      headerName: 'Action',
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
constructor(private readonly adminService: Service, private readonly toastr: ToastrService, private readonly router: Router, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadSuppliers();
  }

  onGridReady(params: any) {
    this.gridApi = params.api;
  }
  
  loadSuppliers(){
    this.loading = true;
    this.adminService.getAllSuppliers().subscribe({
    next: (res) => {
      if (res?.response && Array.isArray(res.response)) {
        this.allRowData = [...res.response];
        this.rowData = [...this.allRowData];
        this.statuses = [...new Set(this.allRowData.map((s: any) => s.status).filter(Boolean))] as string[];
      }
      this.loading = false;
      this.cdr.detectChanges();
    },
    error: (err) => {
      console.error('Error fetching suppliers:', err);
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
      this.router.navigate(['/admin/supplier-add'], { state: { editData: data } });
    }

    if (target.classList.contains('bi-trash')) {
      if (confirm(`Delete supplier "${data.companyName}"?`)) {
        this.adminService.deleteSupplier(data.id).subscribe({
          next: () => { this.toastr.success('Supplier deleted'); this.loadSuppliers(); },
          error: (err: any) => this.toastr.error(err?.error?.message || 'Delete failed')
        });
      }
    }
  }

  applyFilters() {
    let filtered = [...this.allRowData];
    if (this.searchText) {
      const s = this.searchText.toLowerCase();
      filtered = filtered.filter(r => r.companyName?.toLowerCase().includes(s) || r.contactEmail?.toLowerCase().includes(s));
    }
    if (this.selectedStatus) {
      filtered = filtered.filter(r => r.status === this.selectedStatus);
    }
    this.rowData = filtered;
    this.cdr.detectChanges();
  }

}
