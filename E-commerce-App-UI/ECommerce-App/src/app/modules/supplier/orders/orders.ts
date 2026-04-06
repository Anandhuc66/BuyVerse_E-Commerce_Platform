import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ColDef, GridApi } from 'ag-grid-community';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../../../components/confirm-dialog/confirm-dialog';
import { OrderDetailsDialog } from '../../../components/order-details-dialog/order-details-dialog';

@Component({
  selector: 'app-supplier-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class SupplierOrders implements OnInit {
  public paginationPageSize = 10;
  public paginationPageSizeSelector: number[] = [10, 25, 50];
  loading = true;
  private gridApi!: GridApi;

  columnDefs: ColDef[] = [
    { headerName: 'Order #', field: 'orderNumber', width: 170, cellStyle: { textAlign: 'left' } },
    { headerName: 'Product', field: 'productName', flex: 1, cellStyle: { textAlign: 'left' } },
    { headerName: 'Qty', field: 'quantity', width: 80, cellStyle: { textAlign: 'right' } },
    { headerName: 'Unit Price', field: 'unitPrice', width: 120, cellStyle: { textAlign: 'right' }, valueFormatter: p => '₹' + (p.value?.toLocaleString() || '0') },
    { headerName: 'Total', field: 'lineTotal', width: 120, cellStyle: { textAlign: 'right' }, valueFormatter: p => '₹' + (p.value?.toLocaleString() || '0') },
    { headerName: 'Order Date', field: 'orderDate', width: 130, cellStyle: { textAlign: 'center' }, valueFormatter: p => p.value ? new Date(p.value).toLocaleDateString('en-GB') : '' },
    {
      headerName: 'Payment', field: 'paymentStatus', width: 120,
      cellStyle: (params: any) => ({
        textAlign: 'center',
        color: this.getPaymentColor(params.value),
        fontWeight: '600'
      })
    },
    {
      headerName: 'Delivery', field: 'deliveryStatus', width: 130,
      cellStyle: (params: any) => ({
        textAlign: 'center',
        color: this.getDeliveryColor(params.value),
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
        return `
          <select class="delivery-status-select" data-order-id="${params.data?.orderId}" data-current-status="${current}">
            <option value="Pending" ${current === 'Pending' ? 'selected' : ''}>Pending</option>
            <option value="Processing" ${current === 'Processing' ? 'selected' : ''}>Processing</option>
            <option value="Shipped" ${current === 'Shipped' ? 'selected' : ''}>Shipped</option>
            <option value="Delivered" ${current === 'Delivered' ? 'selected' : ''}>Delivered</option>
          </select>`;
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
  paymentMethods: string[] = [];
  paymentStatuses = ['Paid', 'COD', 'Pending'];
  deliveryStatusOptions = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

  constructor(
    private readonly supplierService: Service,
    private readonly toastr: ToastrService,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  onGridReady(params: any): void {
    this.gridApi = params.api;
    const gridEl = document.querySelector('.ag-theme-quartz');
    if (gridEl) {
      gridEl.addEventListener('change', (e: any) => {
        const target = e.target;
        if (target?.classList.contains('delivery-status-select')) {
          const orderId = +target.dataset['orderId'];
          const newStatus = target.value;
          const currentStatus = target.dataset['currentStatus'];
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
    this.supplierService.getSupplierOrders().subscribe({
      next: (res: any) => {
        this.loading = false;
        const orders = res?.response || [];
        const rows: any[] = [];
        orders.forEach((o: any) => {
          (o.products || []).forEach((p: any) => {
            rows.push({
              orderId: o.orderId,
              orderNumber: o.orderNumber,
              productName: p.productName,
              quantity: p.quantity,
              unitPrice: p.unitPrice,
              lineTotal: (p.unitPrice || 0) * (p.quantity || 0),
              orderDate: o.orderDate,
              paymentMethod: o.paymentMethod || '',
              paymentStatus: o.status,
              deliveryStatus: o.deliveryStatus || 'Pending'
            });
          });
        });
        this.rowData = rows;
        this.allRowData = [...this.rowData];
        this.paymentMethods = [...new Set(this.allRowData.map((r: any) => r.paymentMethod).filter(Boolean))] as string[];
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load orders');
        this.cdr.markForCheck();
      }
    });
  }

  onStatusChange(orderId: number, newStatus: string): void {
    if (!orderId) return;
    this.supplierService.updateOrderStatus(orderId, newStatus).subscribe({
      next: () => {
        this.toastr.success(`Delivery status updated to ${newStatus}`);
        this.rowData = this.rowData.map(r =>
          r.orderId === orderId ? { ...r, deliveryStatus: newStatus } : r
        );
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.toastr.error(err?.error?.message || 'Failed to update status');
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
          this.supplierService.deleteOrder(data.orderId).subscribe({
            next: () => { this.toastr.success('Order deleted'); this.loadOrders(); },
            error: (err: any) => this.toastr.error(err?.error?.message || 'Delete failed')
          });
        }
      });
    }
  }

  getPaymentColor(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'paid': return '#28a745';
      case 'cod': return '#42a5f5';
      case 'pending': return '#ffb74d';
      default: return '#6c757d';
    }
  }

  getDeliveryColor(status: string): string {
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
      filtered = filtered.filter(r => r.orderNumber?.toLowerCase().includes(s) || r.productName?.toLowerCase().includes(s));
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
