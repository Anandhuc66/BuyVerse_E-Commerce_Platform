import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ColDef } from 'ag-grid-community';
import { Service } from '../service';

@Component({
  selector: 'app-user',
  standalone: false,
  templateUrl: './user.html',
  styleUrl: './user.css'
})
export class User implements OnInit {
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];

  columnDefs: ColDef[] = [
    { headerName: 'Sl No.', valueGetter: 'node.rowIndex + 1', width: 90, cellStyle: { textAlign: 'right' } },
    { headerName: 'Full Name', field: 'fullName', sortable: true, filter: true, flex: 1, cellStyle: { textAlign: 'left' } },
    { headerName: 'Email', field: 'email', sortable: true, filter: true, flex: 1, cellStyle: { textAlign: 'left' } },
    { headerName: 'Phone', field: 'phoneNumber', sortable: true, filter: true, width: 150, cellStyle: { textAlign: 'left' } },
    { headerName: 'Gender', field: 'gender', sortable: true, width: 110, cellStyle: { textAlign: 'left' } },
    { headerName: 'Role', field: 'role', sortable: true, filter: true, width: 130, cellStyle: { textAlign: 'left' } },
    {
      headerName: 'Joined', field: 'createdAt', width: 140,
      cellStyle: { textAlign: 'center' },
      valueFormatter: (params: any) => params.value ? new Date(params.value).toLocaleDateString('en-GB') : ''
    }
  ];
  rowData: any[] = [];
  allRowData: any[] = [];
  searchText = '';
  selectedRole = '';
  selectedGender = '';
  roles: string[] = [];
  genders: string[] = [];
  loading = true;

  constructor(private readonly adminService: Service, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: (res: any) => {
        this.allRowData = res?.response || [];
        this.rowData = [...this.allRowData];
        this.roles = [...new Set(this.allRowData.map((u: any) => u.role).filter(Boolean))] as string[];
        this.genders = [...new Set(this.allRowData.map((u: any) => u.gender).filter(Boolean))] as string[];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Error fetching users:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters() {
    let filtered = [...this.allRowData];
    if (this.searchText) {
      const s = this.searchText.toLowerCase();
      filtered = filtered.filter(r => r.fullName?.toLowerCase().includes(s) || r.email?.toLowerCase().includes(s));
    }
    if (this.selectedRole) {
      filtered = filtered.filter(r => r.role === this.selectedRole);
    }
    if (this.selectedGender) {
      filtered = filtered.filter(r => r.gender === this.selectedGender);
    }
    this.rowData = filtered;
    this.cdr.detectChanges();
  }
}
