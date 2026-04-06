import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../../../components/confirm-dialog/confirm-dialog';
import { OrderDetailsDialog } from '../../../components/order-details-dialog/order-details-dialog';

@Component({
  selector: 'app-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders implements OnInit {
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
  loading = true;
  private gridApi!: GridApi;

  deliveryStatuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

  columnDefs: ColDef[] = [
    { headerName: 'Order ID', field: 'orderNumber', width: 230, cellStyle: { textAlign: 'left' } },
    { headerName: 'Customer', field: 'customerName', width: 120, cellStyle: { textAlign: 'left' } },
    { headerName: 'Products', field: 'productNames', width: 230, cellStyle: { textAlign: 'left' } },
    { headerName: 'Total (₹)', field: 'totalAmount', width: 120, cellStyle: { textAlign: 'right' }, valueFormatter: p => '₹' + (p.value?.toLocaleString() || '0') },
    { headerName: 'Order Date', field: 'orderDate', width: 130, cellStyle: { textAlign: 'center' }, valueFormatter: p => p.value ? new Date(p.value).toLocaleDateString('en-GB') : '' },
    { headerName: 'Payment', field: 'paymentMethod', width: 120, cellStyle: { textAlign: 'left' } },
    {
      headerName: 'Payment Status', field: 'paymentStatus', width: 140,
      cellStyle: (params: any) => ({
        textAlign: 'center',
        color: this.getPaymentStatusColor(params.value),
        fontWeight: '600'
      })
    },
    {
      headerName: 'Delivery Status', field: 'deliveryStatus', width: 140,
      cellStyle: (params: any) => ({
        textAlign: 'center',
        color: this.getDeliveryStatusColor(params.value),
        fontWeight: '600'
      })
    },
    {
      headerName: 'Update Delivery', width: 180,
      cellStyle: { textAlign: 'center' },
      cellRenderer: (params: any) => {
        const current = params.data?.deliveryStatus || '';
        if (current === 'Delivered' || current === 'Cancelled') {
          return `<span style="color: ${current === 'Delivered' ? '#28a745' : '#dc3545'}; font-weight: 600;">${current}</span>`;
        }
        const options = this.deliveryStatuses
          .map(s => `<option value="${s}" ${s === current ? 'selected' : ''}>${s}</option>`)
          .join('');
        return `
          <select class="delivery-status-select" data-order-id="${params.data?.orderId}" data-current-status="${current}">
            ${options}
          </select>`;
      },
      onCellClicked: (params: any) => {
        // We'll handle via DOM event listener instead
      }
    },
    {
      headerName: 'Actions',
      width: 100,
      cellStyle: { textAlign: 'center' },
      cellRenderer: () => `
        <div style="display:flex; justify-content:center; gap:8px;">
          <i class="bi bi-eye" title="View" style="cursor:pointer; color:#0d6efd;"></i>
          <i class="bi bi-trash" title="Delete" style="cursor:pointer; color:#dc3545;"></i>
        </div>
      `
    }
  ];
  rowData: any[] = [];
  allRowData: any[] = [];
  searchText = '';
  selectedPaymentMethod = '';
  selectedPaymentStatus = '';
  selectedDeliveryStatus = '';
  paymentStatuses = ['Paid', 'COD', 'Pending'];
  paymentMethods: string[] = [];

  constructor(
    private readonly adminService: Service,
    private readonly toastr: ToastrService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  onGridReady(params: any): void {
    this.gridApi = params.api;

    // Listen for change events on the delivery status selects
    const gridEl = document.querySelector('.ag-theme-quartz');
    if (gridEl) {
      gridEl.addEventListener('change', (e: any) => {
        const target = e.target;
        if (target?.classList.contains('delivery-status-select')) {
          const orderId = +target.getAttribute('data-order-id');
          const newStatus = target.value;
          const currentStatus = target.getAttribute('data-current-status');
          const dialogRef = this.dialog.open(ConfirmDialog, {
            width: '400px',
            data: {
              title: 'Update Delivery Status',
              message: `Are you sure you want to change the delivery status to "${newStatus}"?`,
              icon: 'bi-truck',
              confirmText: 'Update',
              cancelText: 'Cancel'
            }
          });
          dialogRef.afterClosed().subscribe(result => {
            if (result) {
              this.onStatusChange(orderId, newStatus);
            } else {
              target.value = currentStatus;
            }
          });
        }
      });
    }
  }

  loadOrders(): void {
    this.loading = true;
    this.adminService.getAllOrders().subscribe({
      next: (res: any) => {
        this.loading = false;
        const orders = res?.response || [];
        this.rowData = orders.map((o: any) => ({
          orderId: o.orderId,
          orderNumber: o.orderNumber,
          customerName: o.user?.fullName || 'N/A',
          totalAmount: o.totalAmount,
          orderDate: o.orderDate,
          paymentMethod: o.payment?.paymentMethod || 'N/A',
          paymentStatus: o.status,
          deliveryStatus: o.deliveryStatus,
          productNames: o.products?.map((p: any) => p.name).join(', ') || 'N/A'
        }));
        this.allRowData = [...this.rowData];
        this.paymentMethods = [...new Set(this.rowData.map((o: any) => o.paymentMethod).filter(Boolean))] as string[];
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.loading = false;
        this.toastr.error('Failed to load orders');
        console.error('Error fetching orders:', err);
        this.cdr.markForCheck();
      }
    });
  }

  onStatusChange(orderId: number, newStatus: string): void {
    if (!orderId) return;
    this.adminService.updateOrderStatus(orderId, newStatus).subscribe({
      next: () => {
        this.toastr.success(`Order #${orderId} status updated to ${newStatus}`);
        // Update the local row data
        const row = this.rowData.find(r => r.orderId === orderId);
        if (row) {
          row.deliveryStatus = newStatus;
          this.rowData = [...this.rowData];
          this.cdr.markForCheck();
        }
      },
      error: (err: any) => {
        this.toastr.error(err?.error?.message || 'Failed to update status');
        // Reload to reset dropdown
        this.loadOrders();
      }
    });
  }

  onCellClicked(event: any): void {
    const target = event.event?.target as HTMLElement;
    if (!target) return;
    const data = event.data;

    if (target.classList.contains('bi-eye')) {
      this.dialog.open(OrderDetailsDialog, {
        width: '550px',
        data: { orderId: data.orderId }
      });
    }

    if (target.classList.contains('bi-trash')) {
      const dialogRef = this.dialog.open(ConfirmDialog, {
        width: '400px',
        data: {
          title: 'Delete Order',
          message: `Are you sure you want to delete order "${data.orderNumber}"?`,
          icon: 'bi-trash',
          confirmText: 'Delete',
          cancelText: 'Cancel'
        }
      });
      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.adminService.deleteOrder(data.orderId).subscribe({
            next: () => { this.toastr.success('Order deleted'); this.loadOrders(); },
            error: (err: any) => this.toastr.error(err?.error?.message || 'Delete failed')
          });
        }
      });
    }
  }

  getPaymentStatusColor(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'paid': return '#28a745';
      case 'cod': return '#2196f3';
      case 'pending': return '#ffb74d';
      default: return '#6c757d';
    }
  }

  getDeliveryStatusColor(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'delivered': return '#28a745';
      case 'shipped': return '#0e917a';
      case 'processing': return '#ff9800';
      case 'pending': return '#6c757d';
      case 'cancelled': return '#dc3545';
      default: return '#6c757d';
    }
  }

  applyFilters() {
    let filtered = [...this.allRowData];
    if (this.searchText) {
      const s = this.searchText.toLowerCase();
      filtered = filtered.filter(r => r.orderNumber?.toLowerCase().includes(s) || r.customerName?.toLowerCase().includes(s));
    }
    if (this.selectedPaymentMethod) {
      filtered = filtered.filter(r => r.paymentMethod === this.selectedPaymentMethod);
    }
    if (this.selectedPaymentStatus) {
      filtered = filtered.filter(r => r.paymentStatus === this.selectedPaymentStatus);
    }
    if (this.selectedDeliveryStatus) {
      filtered = filtered.filter(r => r.deliveryStatus === this.selectedDeliveryStatus);
    }
    this.rowData = filtered;
    this.cdr.markForCheck();
  }
}
