import { Component, OnInit } from '@angular/core';
import {
  trigger,
  transition,
  style,
  animate,
  query,
  stagger,
} from '@angular/animations';
import { Service } from '../service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { Unauthorized } from '../../../components/unauthorized/unauthorized';
import { environment } from '../../../Environment/environment';

interface CartItem {
  id: number;         // CartId
  name: string;       // ProductName
  price: number;      // ProductPrice
  quantity: number;   // Quantity
  image: string;      // Product Image
  productId: number;  // Needed for update API
}

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.html',
  styleUrl: './cart.css',
  animations: [
    trigger('cartStagger', [
      transition('* => *', [
        query(
          ':enter',
          [
            style({ opacity: 0, transform: 'translateY(10px) scale(0.98)' }),
            stagger(
              '80ms',
              animate(
                '400ms cubic-bezier(.2,.8,.2,1)',
                style({ opacity: 1, transform: 'translateY(0) scale(1)' })
              )
            ),
          ],
          { optional: true }
        ),
      ]),
    ]),
  ],
})
export class Cart implements OnInit {
  cartItems: CartItem[] = [];
  baseUrl = environment.assetUrl;
  loading = true;

  constructor(private readonly userService: Service, private readonly router: Router, private readonly toastr: ToastrService, private readonly dialog: MatDialog){}
  
 ngOnInit(): void {
    const userId = localStorage.getItem('userId');
    if (!userId) {
      this.dialog.open(Unauthorized, {
        width: '450px',
        panelClass: 'auth-dialog'
      });
      return;
    }
    this.loadCart();
  }

  loadCart() {
    this.userService.getCartByUserId().subscribe({
      next: (res) => {
        this.cartItems = res.response.map((cart: any) => ({
          id: cart.cartId,
          productId: cart.productId,  // <-- needed for update
          name: cart.productName,
          price: cart.productPrice,
          quantity: cart.quantity,
          image: cart.imageUrls && cart.imageUrls.length > 0
            ? this.baseUrl + cart.imageUrls[0]
            : 'assets/placeholder.png'
        }));
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
    // ================== UPDATE QTY ===================
updateQty(item: CartItem) {
  const userId = localStorage.getItem('userId')!;  // <-- FIXED

  const body = {
    userId,
    productId: item.productId,
    quantity: item.quantity,
    cartId: item.id
  };

  this.userService.updateCart(body).subscribe({
    next: () => {
      this.toastr.success("Cart updated!");
       this.loadCart();
    },
    error: () => {
      this.toastr.error("Failed to update cart");
       this.loadCart();
    }
  });
}


  increaseQty(item: CartItem) {
    item.quantity++;
    this.updateQty(item);
  }

  decreaseQty(item: CartItem) {
    if (item.quantity > 1) {
      item.quantity--;
      this.updateQty(item);
    }
  }


  // ================== DELETE ITEM ===================
  removeItem(cartId: number) {
    this.userService.deleteCartItem(cartId).subscribe({
      next: () => {
        this.cartItems = this.cartItems.filter(i => i.id !== cartId);
        this.toastr.success("Item removed");

        // Header count decrease
        this.userService.decreaseCartCount();
      },
      error: () => {
        this.toastr.error("Failed to remove item");
      }
    });
  }




// cartItems: CartItem[] = [
//     {
//       id: 1,
//       title: 'Urban Sneakers',
//       price: 3499,
//       quantity: 1,
//       image: 'https://picsum.photos/seed/cart1/400/300',
//     },
//     {
//       id: 2,
//       title: 'Leather Wallet',
//       price: 1499,
//       quantity: 2,
//       image: 'https://picsum.photos/seed/cart2/400/300',
//     },
//     {
//       id: 3,
//       title: 'Smart Watch',
//       price: 5999,
//       quantity: 1,
//       image: 'https://picsum.photos/seed/cart3/400/300',
//     },
//   ];

  get totalAmount(): number {
    return this.cartItems.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0
    );
  }

  // increaseQty(item: CartItem) {
  //   item.quantity++;
  // }

  // decreaseQty(item: CartItem) {
  //   if (item.quantity > 1) item.quantity--;
  // }

  // removeItem(id: number) {
  //   this.cartItems = this.cartItems.filter((item) => item.id !== id);
  // }

  checkout() {
  this.router.navigate(
    ['/checkout'],
    {
      state: { cartItems: this.cartItems }  // 👈 pass cart items
    }
  );
}


  trackByItem(index: number, item: any): number {
    return item.id;
  }
}
