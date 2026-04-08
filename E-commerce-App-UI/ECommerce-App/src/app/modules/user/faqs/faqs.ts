import { Component } from '@angular/core';

@Component({
  selector: 'app-faqs',
  standalone: false,
  templateUrl: './faqs.html',
  styleUrl: './faqs.css'
})
export class Faqs {
  faqCategories = [
    {
      title: 'Orders & Payment',
      icon: 'bi-bag-check',
      faqs: [
        { q: 'How do I place an order?', a: 'Browse products, add items to your cart, proceed to checkout, select your shipping address and payment method, then confirm your order. You\'ll receive an email confirmation once your order is placed.', open: false },
        { q: 'What payment methods do you accept?', a: 'We accept Razorpay (credit/debit cards, UPI, net banking, wallets) and Cash on Delivery (COD). All online payments are processed securely through Razorpay.', open: false },
        { q: 'Can I cancel my order?', a: 'Yes, you can cancel your order from "My Orders" as long as the delivery status is still Pending or Processing. Once shipped, cancellation is not possible.', open: false },
        { q: 'How do I apply a discount code?', a: 'Enter your discount code at checkout in the "Apply Coupon" field. The discount will be reflected in your order total before payment.', open: false }
      ]
    },
    {
      title: 'Shipping & Delivery',
      icon: 'bi-truck',
      faqs: [
        { q: 'How long does delivery take?', a: 'Standard delivery takes 5-7 business days. Delivery times may vary based on your location and product availability.', open: false },
        { q: 'How can I track my order?', a: 'Go to "My Orders" in your account to view the real-time status of all your orders including delivery updates.', open: false },
        { q: 'Do you deliver internationally?', a: 'Currently, we deliver across India only. International shipping is not available at this time.', open: false },
        { q: 'What are the shipping charges?', a: 'Shipping charges are calculated based on order value and delivery location. Free shipping may be available on orders above a certain amount.', open: false }
      ]
    },
    {
      title: 'Returns & Refunds',
      icon: 'bi-arrow-return-left',
      faqs: [
        { q: 'What is your return policy?', a: 'Items can be returned within 7 days of delivery if they are unused, in original packaging, and in the same condition as received.', open: false },
        { q: 'How do I initiate a return?', a: 'Go to "My Orders", select the order you wish to return, and follow the return instructions. Our team will arrange a pickup.', open: false },
        { q: 'When will I get my refund?', a: 'Once the returned item is received and inspected, your refund will be processed within 5-7 business days to the original payment method.', open: false }
      ]
    },
    {
      title: 'Account & Security',
      icon: 'bi-shield-lock',
      faqs: [
        { q: 'How do I create an account?', a: 'Click "Register" on the top right, fill in your details (name, email, password), and verify your email to get started.', open: false },
        { q: 'How do I reset my password?', a: 'Go to your Profile page and use the "Change Password" option. You\'ll need to enter your current password and a new password.', open: false },
        { q: 'Is my personal information safe?', a: 'Yes, we use industry-standard encryption and security measures. Your payment data is processed through Razorpay\'s secure gateway and is never stored on our servers.', open: false }
      ]
    }
  ];

  toggleFaq(faq: any) {
    faq.open = !faq.open;
  }
}
