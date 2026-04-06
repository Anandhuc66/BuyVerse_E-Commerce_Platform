import { Injectable } from '@angular/core';
import { environment } from '../../Environment/environment';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Service {
  private readonly _baseUrl = environment.baseUrl;
  private readonly _getAllProducts = 'Product';
  private readonly _getProductById = 'Product/';
  private readonly _addToCartUrl = 'Cart';
  private readonly _getCartByUserId = 'Cart/user/';
  private readonly _updateCartUrl = 'Cart';
  private readonly _deleteCartUrl = 'Cart/';

  // LIVE CART COUNT
  private readonly cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  getAllProduct(): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getAllProducts}`);
  }

  getProductById(id: number): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getProductById}${id}`);
  }

  getProductsBySubCategory(subCategoryId: number): Observable<any> {
    return this.http.get(`${this._baseUrl}Product/SubCategory/${subCategoryId}`);
  }

  searchProducts(keyword: string): Observable<any> {
    return this.http.get(`${this._baseUrl}Product/search?keyword=${encodeURIComponent(keyword)}`);
  }

  // --------------------- CART APIs ----------------------

  addToCart(body: { userId: string; productId: number; quantity: number }): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addToCartUrl}`, body);
  }

  getCartByUserId(): Observable<any> {
    const userId = localStorage.getItem('userId');
    return this.http.get(`${this._baseUrl}${this._getCartByUserId}${userId}`).pipe(
      tap((response: any) => {
        const count = response?.response?.length ?? 0;
        this.cartCountSubject.next(count);
      })
    );
  }

  updateCart(body: { userId: string; productId: number; quantity: number; cartId: number }): Observable<any> {
    return this.http.put(`${this._baseUrl}${this._updateCartUrl}`, body);
  }

  deleteCartItem(cartId: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}${this._deleteCartUrl}${cartId}`).pipe(
      tap(() => this.decreaseCartCount())
    );
  }

  // --------------------- CART COUNT HELPERS ----------------------

  increaseCartCount() {
    this.cartCountSubject.next(this.cartCountSubject.value + 1);
  }

  decreaseCartCount() {
    if (this.cartCountSubject.value > 0) {
      this.cartCountSubject.next(this.cartCountSubject.value - 1);
    }
  }

  // --------------------- PRODUCT REVIEW ----------------------

  submitReview(body: {
    productId: number;
    userId: string;
    rating: number;
    title: string;
    comment: string;
  }): Observable<any> {
    return this.http.post(`${this._baseUrl}ProductReview`, body);
  }

  getReviews(productId: number): Observable<any> {
    return this.http.get(`${this._baseUrl}ProductReview/product/${productId}`);
  }

  // --------------------- ORDER ----------------------
  createOrder(body: any): Observable<any> {
    return this.http.post(`${this._baseUrl}Order`, body);
  }

  getOrdersByUser(userId: string): Observable<any> {
    return this.http.get(`${this._baseUrl}Order/user/${userId}`);
  }

  cancelOrder(orderId: number): Observable<any> {
    return this.http.put(`${this._baseUrl}Order/${orderId}/status`, { deliveryStatus: 'Cancelled' });
  }

  deleteOrder(orderId: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Order/${orderId}`);
  }

  // --------------------- RAZORPAY ----------------------
  createRazorpayOrder(orderId: number): Observable<any> {
    return this.http.post(`${this._baseUrl}razorpay/create-order`, { orderId });
  }

  verifyRazorpayPayment(data: any): Observable<any> {
    return this.http.post(`${this._baseUrl}razorpay/verify`, data);
  }

  // --------------------- USER ADDRESS ----------------------
  getUserAddresses(userId: string): Observable<any> {
    return this.http.get(`${this._baseUrl}UserAddress/user/${userId}`);
  }

  addUserAddress(body: {
    userId: string;
    fullAddress: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
  }): Observable<any> {
    return this.http.post(`${this._baseUrl}UserAddress`, body);
  }

  // --------------------- INVOICE ----------------------
  downloadInvoice(orderId: number) {
    return this.http.get(`${this._baseUrl}Order/${orderId}/invoice`, {
      responseType: 'blob'
    });
  }

  // --------------------- CHANGE PASSWORD ----------------------
  changePassword(body: { currentPassword: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this._baseUrl}Auth/change-password`, body);
  }
}
