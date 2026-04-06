import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-roles',
  standalone: false,
  templateUrl: './roles.html',
  styleUrl: './roles.css'
})
export class Roles {
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
  rowData: any[] = [];
  loading = true;
  allRowData: any[] = [];
  searchText = '';
  filterDate = '';
    private gridApi!: GridApi;



columnDefs: ColDef[] = [
  { headerName: 'Sl. No', valueGetter: 'node.rowIndex + 1', width: 100, cellStyle: { textAlign: 'right' } },
   { headerName: 'Role Id', field: 'id', width:280, cellStyle: { textAlign: 'left' } },
  { headerName: 'Role Name', field: 'name', flex: 1, cellStyle: { textAlign: 'left' } },
  { headerName: 'Display Name', field: 'displayName', flex: 1, cellStyle: { textAlign: 'left' } },
  { headerName: 'Description', field: 'description', flex: 2, cellStyle: { textAlign: 'left' } },
  { 
    headerName: 'Created Date', 
    field: 'createdAt', 
    width: 200,
    cellStyle: { textAlign: 'center' },
    valueFormatter: (params) => 
      params.value ? new Date(params.value).toLocaleDateString('en-GB') : '--'
  },
  { 
    headerName: 'Action', 
    width: 150,
    cellStyle: { textAlign: 'center' },
    cellRenderer: () => `
      <div style="display:flex; justify-content:center; gap:8px;">
        <i class="bi bi-pencil" title="Edit" style="cursor:pointer; color:#198754;"></i>
        <i class="bi bi-trash" title="Delete" style="cursor:pointer; color:#dc3545;"></i>
      </div>
    `
  }
];


constructor(private readonly adminService: Service, private readonly toastr: ToastrService, private readonly router: Router, private readonly cdr: ChangeDetectorRef) {}

 ngOnInit() {
    this.loadRole();
  }

  onGridReady(params: any) {
    this.gridApi = params.api;
  }

  loadRole() {
  this.loading = true;
  this.adminService.getAllRole().subscribe({
    next: (res) => {
      if (res?.response && Array.isArray(res.response)) {
        this.allRowData = [...res.response];
        this.rowData = [...this.allRowData];
      }
      this.loading = false;
      this.cdr.detectChanges();
    },
    error: (err) => {
      console.error('Error fetching roles:', err);
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
      this.router.navigate(['/admin/role-add'], { state: { editData: data } });
    }

    if (target.classList.contains('bi-trash')) {
      if (confirm(`Delete role "${data.name}"?`)) {
        this.adminService.deleteRole(data.id).subscribe({
          next: () => { this.toastr.success('Role deleted'); this.loadRole(); },
          error: (err: any) => this.toastr.error(err?.error?.message || 'Delete failed')
        });
      }
    }
  }

  applyFilters() {
    let filtered = [...this.allRowData];
    if (this.searchText) {
      const s = this.searchText.toLowerCase();
      filtered = filtered.filter(r => r.name?.toLowerCase().includes(s) || r.displayName?.toLowerCase().includes(s));
    }
    if (this.filterDate) {
      const d = new Date(this.filterDate);
      filtered = filtered.filter(r => r.createdAt && new Date(r.createdAt) >= d);
    }
    this.rowData = filtered;
    this.cdr.detectChanges();
  }

}
