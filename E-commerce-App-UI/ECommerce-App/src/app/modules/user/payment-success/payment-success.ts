import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Service } from '../service';

@Component({
  selector: 'app-payment-success',
  standalone: false,
  templateUrl: './payment-success.html',
  styleUrl: './payment-success.css'
})
export class PaymentSuccess  implements OnInit{

  orderId!: number;
  orderNumber!: string;
  paymentId!: string;
  amount!: number;

  estimatedDelivery = '';

  constructor(
    private readonly router: Router,
    private readonly service: Service
  ) {}

  ngOnInit() {
    const state = history.state;

    // Guard: redirect if navigated directly without order data
    if (!state?.orderNumber && !state?.orderId) {
      this.router.navigate(['/']);
      return;
    }

    this.orderId = state.orderId;
    this.orderNumber = state.orderNumber;
    this.paymentId = state.paymentId;
    this.amount = state.amount;

    // Example: Estimated Delivery = today + 5 days
    const date = new Date();
    date.setDate(date.getDate() + 5);
    this.estimatedDelivery = date.toDateString();
  }

  goToOrders() {
    this.router.navigate(['/orders']);
  }

  downloadInvoice() {
    this.service.downloadInvoice(this.orderId).subscribe({
      next: (pdf) => {
        const blob = new Blob([pdf], { type: 'application/pdf' });
        const url = globalThis.URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice_${this.orderNumber}.pdf`;
        a.click();

        globalThis.URL.revokeObjectURL(url);
      },
      error: () => {
        alert('Failed to download invoice. Please try again.');
      }
    });
  }

  continueShopping() {
    this.router.navigate(['/']);
  }
}
