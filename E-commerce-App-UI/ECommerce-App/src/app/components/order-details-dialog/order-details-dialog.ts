import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../Environment/environment';

@Component({
  selector: 'app-order-details-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="order-dialog">
      <div class="dialog-header">
        <div class="header-icon">
          <i class="bi bi-receipt-cutoff"></i>
        </div>
        <h2>Order Details</h2>
        <button class="close-btn" (click)="close()"><i class="bi bi-x-lg"></i></button>
      </div>

      <div *ngIf="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading order details...</p>
      </div>

      <div *ngIf="!loading && order" class="dialog-body">
        <!-- Order Info -->
        <div class="info-section">
          <h6 class="section-title"><i class="bi bi-box-seam me-2"></i>Order Info</h6>
          <div class="info-grid">
            <div class="info-item"><span class="label">Order #</span><span class="value">{{ order.orderNumber }}</span></div>
            <div class="info-item"><span class="label">Date</span><span class="value">{{ order.orderDate | date:'dd/MM/yyyy' }}</span></div>
            <div class="info-item"><span class="label">Total</span><span class="value fw-bold">\u20b9{{ order.totalAmount | number }}</span></div>
            <div class="info-item"><span class="label">Payment</span><span class="value">{{ order.payment?.paymentMethod || 'N/A' }}</span></div>
            <div class="info-item">
              <span class="label">Payment Status</span>
              <span class="value badge" [class]="getPaymentBadge(order.status)">{{ order.status }}</span>
            </div>
            <div class="info-item">
              <span class="label">Delivery</span>
              <span class="value badge" [class]="getDeliveryBadge(order.deliveryStatus)">{{ order.deliveryStatus }}</span>
            </div>
          </div>
        </div>

        <!-- Customer Info -->
        <div class="info-section" *ngIf="order.user">
          <h6 class="section-title"><i class="bi bi-person me-2"></i>Customer</h6>
          <div class="info-grid">
            <div class="info-item"><span class="label">Name</span><span class="value">{{ order.user.fullName }}</span></div>
            <div class="info-item"><span class="label">Email</span><span class="value">{{ order.user.email }}</span></div>
            <div class="info-item"><span class="label">Phone</span><span class="value">{{ order.user.phoneNumber || 'N/A' }}</span></div>
          </div>
        </div>

        <!-- Shipping Address -->
        <div class="info-section" *ngIf="order.shippingAddress">
          <h6 class="section-title"><i class="bi bi-geo-alt me-2"></i>Shipping Address</h6>
          <p class="address-text">
            {{ order.shippingAddress.fullAddress }}<br>
            {{ order.shippingAddress.city }}, {{ order.shippingAddress.state }} - {{ order.shippingAddress.zipCode }}<br>
            {{ order.shippingAddress.country }}
          </p>
        </div>

        <!-- Products -->
        <div class="info-section">
          <h6 class="section-title"><i class="bi bi-cart3 me-2"></i>Products</h6>
          <div class="product-list">
            <div class="product-item" *ngFor="let p of order.products">
              <div class="product-info">
                <span class="product-name">{{ p.name || p.productName }}</span>
                <span class="product-qty">x{{ p.quantity }}</span>
              </div>
              <span class="product-price">\u20b9{{ (p.unitPrice * p.quantity) | number }}</span>
            </div>
          </div>
        </div>

        <!-- Payment Info -->
        <div class="info-section" *ngIf="order.payment">
          <h6 class="section-title"><i class="bi bi-credit-card me-2"></i>Payment</h6>
          <div class="info-grid">
            <div class="info-item"><span class="label">Method</span><span class="value">{{ order.payment.paymentMethod }}</span></div>
            <div class="info-item"><span class="label">Transaction ID</span><span class="value">{{ order.payment.transactionId || 'N/A' }}</span></div>
            <div class="info-item"><span class="label">Date</span><span class="value">{{ order.payment.paymentDate | date:'dd/MM/yyyy' }}</span></div>
            <div class="info-item"><span class="label">Status</span><span class="value">{{ order.payment.status }}</span></div>
          </div>
        </div>
      </div>

      <div *ngIf="!loading && !order" class="error-state">
        <i class="bi bi-exclamation-circle"></i>
        <p>Failed to load order details.</p>
      </div>
    </div>
  `,
  styles: [`
    .order-dialog { max-height: 80vh; overflow-y: auto; }
    .dialog-header {
      display: flex; align-items: center; gap: 12px;
      padding: 20px 24px; border-bottom: 1px solid #E2E8F0;
      position: sticky; top: 0; background: #fff; z-index: 1;
    }
    .header-icon {
      width: 40px; height: 40px; border-radius: 50%;
      background: #e8f0fe; display: flex; align-items: center; justify-content: center;
    }
    .header-icon i { font-size: 20px; color: var(--button); }
    .dialog-header h2 { margin: 0; font-size: 18px; font-weight: 600; flex: 1; }
    .close-btn {
      background: none; border: none; cursor: pointer;
      width: 32px; height: 32px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      transition: background 0.2s;
    }
    .close-btn:hover { background: #F1F5F9; }
    .close-btn i { font-size: 16px; color: #64748B; }

    .dialog-body { padding: 16px 24px 24px; }

    .info-section {
      margin-bottom: 20px; padding-bottom: 16px;
      border-bottom: 1px solid #F1F5F9;
    }
    .info-section:last-child { border-bottom: none; margin-bottom: 0; }

    .section-title {
      font-size: 13px; font-weight: 600; text-transform: uppercase;
      letter-spacing: 0.5px; color: var(--button); margin-bottom: 12px;
    }

    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
    .info-item { display: flex; flex-direction: column; gap: 2px; }
    .label { font-size: 11px; color: #94A3B8; text-transform: uppercase; letter-spacing: 0.3px; }
    .value { font-size: 14px; color: #1E293B; }

    .badge {
      display: inline-block; padding: 2px 8px; border-radius: 4px;
      font-size: 12px; font-weight: 600; width: fit-content;
    }
    .badge-success { background: #DCFCE7; color: #166534; }
    .badge-warning { background: #FEF3C7; color: #92400E; }
    .badge-info { background: #DBEAFE; color: #1E40AF; }
    .badge-danger { background: #FEE2E2; color: #991B1B; }
    .badge-secondary { background: #F1F5F9; color: #475569; }

    .address-text { font-size: 14px; color: #334155; line-height: 1.6; margin: 0; }

    .product-list { display: flex; flex-direction: column; gap: 8px; }
    .product-item {
      display: flex; justify-content: space-between; align-items: center;
      padding: 8px 12px; background: #F8FAFC; border-radius: 6px;
    }
    .product-info { display: flex; align-items: center; gap: 8px; }
    .product-name { font-size: 14px; color: #1E293B; }
    .product-qty { font-size: 12px; color: #94A3B8; }
    .product-price { font-size: 14px; font-weight: 600; color: #1E293B; }

    .loading-state, .error-state {
      padding: 40px; text-align: center; color: #64748B;
    }
    .spinner {
      width: 32px; height: 32px; border: 3px solid #E2E8F0;
      border-top-color: var(--button); border-radius: 50%;
      animation: spin 0.8s linear infinite; margin: 0 auto 12px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .error-state i { font-size: 40px; color: #dc3545; display: block; margin-bottom: 12px; }
    .fw-bold { font-weight: 700; }
    .me-2 { margin-right: 8px; }
  `]
})
export class OrderDetailsDialog implements OnInit {
  order: any = null;
  loading = true;
  private baseUrl = environment.baseUrl;

  constructor(
    public dialogRef: MatDialogRef<OrderDetailsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { orderId: number },
    private readonly http: HttpClient,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.http.get(`${this.baseUrl}Order/${this.data.orderId}`).subscribe({
      next: (res: any) => {
        this.order = res.response;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  close() {
    this.dialogRef.close();
  }

  getPaymentBadge(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'paid': return 'badge-success';
      case 'cod': return 'badge-info';
      case 'pending': return 'badge-warning';
      default: return 'badge-secondary';
    }
  }

  getDeliveryBadge(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'delivered': return 'badge-success';
      case 'shipped': return 'badge-info';
      case 'processing': return 'badge-warning';
      case 'cancelled': return 'badge-danger';
      default: return 'badge-secondary';
    }
  }
}
