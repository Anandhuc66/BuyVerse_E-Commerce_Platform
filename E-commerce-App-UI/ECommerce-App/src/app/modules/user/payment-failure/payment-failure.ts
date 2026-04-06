import { Component } from '@angular/core';
import { Router } from '@angular/router';


@Component({
  selector: 'app-payment-failure',
  standalone: false,
  templateUrl: './payment-failure.html',
  styleUrl: './payment-failure.css'
})
export class PaymentFailure {
  constructor(private readonly router: Router) {}

  tryAgain(): void {
    this.router.navigate(['/checkout']);
  }
}
