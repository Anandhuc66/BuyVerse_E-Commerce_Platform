import { Injectable } from '@angular/core';
import { environment } from '../../Environment/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Service {
   private readonly _baseUrl = environment.baseUrl;
   private readonly _addProductWithImage = 'Product/add-with-images';
   private readonly _getProductByUserId = 'Supplier/GetSupplierByUserId/';

   constructor(private readonly http: HttpClient) {}

  addProduct(formData: FormData): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addProductWithImage}`, formData);
  }

  getProductByUserId(): Observable<any> {
    const userId = localStorage.getItem('userId');
    return this.http.get(`${this._baseUrl}${this._getProductByUserId}${userId}`);
  }

  getSupplierOrders(): Observable<any> {
    const supplierId = localStorage.getItem('supplierId');
    return this.http.get(`${this._baseUrl}Order/supplier/${supplierId}`);
  }

  updateOrderStatus(orderId: number, status: string): Observable<any> {
    return this.http.put(`${this._baseUrl}Order/${orderId}/status`, { DeliveryStatus: status });
  }

  deleteOrder(orderId: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Order/${orderId}`);
  }

  updateProduct(data: any): Observable<any> {
    return this.http.put(`${this._baseUrl}Product`, data);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Product/${id}`);
  }

  getProductsBySupplier(supplierId: number): Observable<any> {
    return this.http.get(`${this._baseUrl}Product/BySupplier/${supplierId}`);
  }
}
