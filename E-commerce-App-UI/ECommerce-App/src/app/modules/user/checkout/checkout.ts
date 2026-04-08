import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Service } from '../service';

interface CartItem {
  productId: number;
  name: string;
  price: number;
  quantity: number;
  image?: string; // 👈 Add image
}

declare var Razorpay: any;

@Component({
  selector: 'app-checkout',
  standalone: false,
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class Checkout implements OnInit {
 
  cartItems: CartItem[] = [];
  subtotal = 0;
  shipping = 0;
  total = 0;

  userAddresses: any[] = [];
  shippingAddressId: number = 0;

  paymentMethod: string = "razorpay";
  isProcessing = false;
  loading = true;

  address = {
  fullAddress: "",
  city: "",
  state: "",
  zipCode: "",
  country: ""
};


  constructor(private readonly router: Router, private readonly userService: Service, private readonly cdr: ChangeDetectorRef) {}

ngOnInit(): void {
  const userId = localStorage.getItem('userId');

  // Load saved addresses
  if (userId) {
    this.userService.getUserAddresses(userId).subscribe({
      next: (res) => {
      this.userAddresses = res.response;

      if (this.userAddresses.length > 0) {
        const addr = this.userAddresses[0];  // take the first saved address

        this.shippingAddressId = addr.id;

        // ✅ Auto-fill input fields
        this.address.fullAddress = addr.fullAddress;
        this.address.city = addr.city;
        this.address.state = addr.state;
        this.address.zipCode = addr.zipCode;
        this.address.country = addr.country;
      }
      this.loading = false;
      this.cdr.detectChanges();
      },
      error: () => {
        this.userAddresses = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  } else {
    this.loading = false;
    this.cdr.detectChanges();
  }

  // 1. Checkout from cart page
  const cartItemsFromState = history.state.cartItems;
  if (cartItemsFromState) {
    this.cartItems = cartItemsFromState;
    this.calculateTotals();
    return;
  }

  // 2. Buy Now (single product)
  const product = history.state.product;
  if (product) {
    this.cartItems = [{
      productId: product.id,
      name: product.title,
      price: product.price,
      quantity: product.quantity,
      image: product.images ? product.images[0] : product.image
    }];
    this.calculateTotals();
    return;
  }

  // 3. Opening /checkout directly
  this.cartItems = [];
  this.calculateTotals();
}


  calculateTotals() {
    this.subtotal = this.cartItems.reduce((sum, item) =>
      sum + (item.price * item.quantity), 0
    );
    this.total = this.subtotal + this.shipping;
  }

  // Navigate to Add Address Page
  goToAddAddress() {
    this.router.navigate(['/user/add-address']);
  }

placeOrder() {
  if (this.isProcessing) return;

  // Guard: prevent checkout with empty cart
  if (!this.cartItems || this.cartItems.length === 0) {
    alert('Your cart is empty. Add items before checking out.');
    return;
  }

  this.isProcessing = true;

  const userId = localStorage.getItem("userId") ?? "";

  // If user selected existing address, do NOT create new one
  if (this.shippingAddressId > 0) {
    this.createOrderAndPay(this.shippingAddressId);
    return;
  }

  // Validate manual address fields
  if (!this.address.fullAddress || !this.address.city || 
      !this.address.state || !this.address.zipCode || 
      !this.address.country) {

    alert("Please fill all address fields.");
    this.isProcessing = false;
    return;
  }

  // 1️⃣ Create new address if fields are entered manually
  const addressPayload = {
    userId,
    fullAddress: this.address.fullAddress,
    city: this.address.city,
    state: this.address.state,
    zipCode: this.address.zipCode,
    country: this.address.country
  };

  this.userService.addUserAddress(addressPayload).subscribe({
    next: (addrRes: any) => {
      const newAddressId = addrRes.response.id;
      this.createOrderAndPay(newAddressId);
    },
    error: () => {
      this.isProcessing = false;
      alert('Failed to save address. Please try again.');
    }
  });
}

createOrderAndPay(shippingAddressId: number) {
  const userId = localStorage.getItem("userId") ?? "";

  const orderDetails = this.cartItems.map(item => ({
  productId: item.productId,   // ✅ Correct product ID
  quantity: item.quantity
}));

  const orderPayload = {
    userId,
    shippingAddressId,
    orderDetails,
    status: this.paymentMethod === "cod" ? "COD" : "Pending"
  };

  // COD
  if (this.paymentMethod === "cod") {
    this.userService.createOrder(orderPayload).subscribe({
      next: () => {
        this.isProcessing = false;
        this.router.navigate(['/payment-success'], {
          state: { orderNumber: 'COD Order', amount: this.total }
        });
      },
      error: () => {
        this.isProcessing = false;
        alert('Failed to place order. Please try again.');
      }
    });
    return;
  }

  // Razorpay
  this.userService.createOrder(orderPayload).subscribe({
    next: (orderRes: any) => {
      const orderId = orderRes.response.id;
      this.initiateRazorpay(orderId, orderRes.response.orderNumber);
    },
    error: () => {
      this.isProcessing = false;
      alert('Failed to create order. Please try again.');
    }
  });
}

private initiateRazorpay(orderId: number, orderNumber: string): void {
  this.userService.createRazorpayOrder(orderId).subscribe({
    next: (rzpRes: any) => {
      const options = {
        key: rzpRes.keyId,
        amount: rzpRes.amount,
        currency: rzpRes.currency,
        order_id: rzpRes.razorpayOrderId,

        handler: (response: any) => {
          this.verifyRazorpayPayment(response, orderId, orderNumber);
        },

        modal: {
          ondismiss: () => {
            this.cancelUnpaidOrder(orderId);
          }
        },

        theme: { color: "#3399cc" }
      };

      const rzpInstance = new Razorpay(options);

      rzpInstance.on('payment.failed', () => {
        this.cancelUnpaidOrder(orderId);
      });

      rzpInstance.open();
    },
    error: () => {
      this.isProcessing = false;
      alert('Failed to initiate Razorpay payment. Please try again.');
    }
  });
}

private verifyRazorpayPayment(response: any, orderId: number, orderNumber: string): void {
  const verifyBody = {
    razorpayPaymentId: response.razorpay_payment_id,
    razorpayOrderId: response.razorpay_order_id,
    razorpaySignature: response.razorpay_signature,
    orderId
  };

  this.userService.verifyRazorpayPayment(verifyBody).subscribe({
    next: () => {
      this.isProcessing = false;
      this.router.navigate(['/payment-success'], {
        state: {
          orderId: orderId,
          orderNumber: orderNumber,
          paymentId: response.razorpay_payment_id,
          amount: this.total
        }
      });
    },
    error: () => {
      this.isProcessing = false;
      alert('Payment verification failed. Contact support.');
    }
  });
}

private cancelUnpaidOrder(orderId: number): void {
  this.isProcessing = false;
  this.userService.cancelOrder(orderId).subscribe({
    next: () => {
      alert('Payment was not completed. Your order has been cancelled.');
    },
    error: () => {
      alert('Payment was not completed. Please cancel the pending order from My Orders.');
    }
  });
}

  trackByCartItem(index: number, item: any): number {
    return item.id || index;
  }
}
