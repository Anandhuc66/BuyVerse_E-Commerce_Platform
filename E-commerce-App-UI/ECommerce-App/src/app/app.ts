import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  protected title = 'ECommerce-App';
  categories = [
    { name: 'Men', image: 'assets/images/men.jpg' },
    { name: 'Women', image: 'assets/images/women.jpg' },
    { name: 'Electronics', image: 'assets/images/electronics.jpg' },
    { name: 'Accessories', image: 'assets/images/accessories.jpg' },
  ];

  products = [
    { name: 'Wireless Headphones', price: 2999, image: 'assets/images/headphones.jpg' },
    { name: 'Smart Watch', price: 4999, image: 'assets/images/watch.jpg' },
    { name: 'Sneakers', price: 2599, image: 'assets/images/shoes.jpg' },
    { name: 'Backpack', price: 1599, image: 'assets/images/bag.jpg' },
  ];
}
