import { Component, OnInit } from '@angular/core';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../Environment/environment';

interface OrderItem {
  productName: string;
  quantity: number;
  price: number;
  productImage?: string;
}

interface TrackingStep {
  title: string;
  desc: string;
  icon: string;
  completed: boolean;
  date: string;
}

interface Order {
  id: number;
  orderNumber: string;
  date: Date;
  paymentStatus: string;
  deliveryStatus: string;
  total: number;
  paymentMethod: string;
  shippingAddress: string;
  items: OrderItem[];
  trackingSteps?: TrackingStep[];
}

@Component({
  selector: 'app-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrls: ['./orders.css']
})
export class Orders implements OnInit {
  orders: Order[] = [];
  openOrderId: number | null = null;
  expandedOrder: number | null = null;
  loading = true;
  baseUrl = environment.assetUrl;

  constructor(private readonly userService: Service, private readonly toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    const userId = localStorage.getItem('userId');
    if (!userId) return;

    this.loading = true;
    this.userService.getOrdersByUser(userId).subscribe({
      next: (res: any) => {
        this.loading = false;
        const orderList = res?.response || [];
        this.orders = orderList.map((o: any) => ({
          id: o.orderId,
          orderNumber: o.orderNumber,
          date: new Date(o.orderDate),
          paymentStatus: o.status,
          deliveryStatus: o.deliveryStatus || 'Pending',
          total: o.totalAmount,
          paymentMethod: o.paymentMethod || 'N/A',
          shippingAddress: o.shippingAddress || 'N/A',
          items: (o.products || []).map((p: any) => ({
            productName: p.productName,
            quantity: p.quantity,
            price: p.unitPrice,
            productImage: p.productImage
          })),
          trackingSteps: this.buildTrackingSteps(o.deliveryStatus || 'Pending', o.orderDate)
        }));
      },
      error: () => {
        this.loading = false;
        this.orders = [];
      }
    });
  }

  buildTrackingSteps(status: string, orderDate: string): TrackingStep[] {
    const statuses = ['Pending', 'Processing', 'Shipped', 'Delivered'];
    const statusIndex = statuses.indexOf(status);

    return statuses.map((s, i) => ({
      title: s === 'Pending' ? 'Order Placed' : s,
      desc: s === 'Pending' ? 'Your order has been placed.' :
            s === 'Processing' ? 'We are preparing your order.' :
            s === 'Shipped' ? 'Your order is on the way.' :
            'Your order has been delivered.',
      icon: s === 'Pending' ? 'bi bi-bag-check' :
            s === 'Processing' ? 'bi bi-gear' :
            s === 'Shipped' ? 'bi bi-truck' : 'bi bi-house-door',
      completed: i <= statusIndex,
      date: i <= statusIndex ? new Date(orderDate).toLocaleDateString() : 'Pending'
    }));
  }

  cancelOrder(orderId: number): void {
    if (!confirm('Are you sure you want to cancel this order?')) return;

    this.userService.cancelOrder(orderId).subscribe({
      next: () => {
        this.toastr.success('Order cancelled successfully');
        this.loadOrders();
      },
      error: () => this.toastr.error('Failed to cancel order')
    });
  }

  deleteOrder(orderId: number): void {
    if (!confirm('Are you sure you want to delete this cancelled order?')) return;

    this.userService.deleteOrder(orderId).subscribe({
      next: () => {
        this.toastr.success('Order deleted successfully');
        this.loadOrders();
      },
      error: () => this.toastr.error('Failed to delete order')
    });
  }

  downloadInvoice(orderId: number): void {
    this.userService.downloadInvoice(orderId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice_${orderId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => this.toastr.error('Failed to download invoice')
    });
  }

  toggleOrder(orderId: number) {
    this.expandedOrder = this.expandedOrder === orderId ? null : orderId;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  isCurrentStep(order: Order, index: number): boolean {
    const step = order.trackingSteps![index];
    if (step.completed) return false;
    return order.trackingSteps!.slice(0, index).every(s => s.completed);
  }

  trackByOrder(index: number, order: any): number {
    return order.id;
  }

  trackByOrderItem(index: number, item: any): number {
    return item.id || index;
  }
}
