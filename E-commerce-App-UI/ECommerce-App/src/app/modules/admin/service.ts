import { Injectable } from '@angular/core';
import { environment } from '../../Environment/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Service {
  private readonly _baseUrl = environment.baseUrl;
  private readonly _getAllProducts = 'Product';
  private readonly _getAllRoles = 'Role';
  private readonly _getAllCategory = 'Category';
  private readonly _getSubCategoryByCategory = 'SubCategory/ByCategory/';
  private readonly _getAllSuppliers = 'Supplier';
  private readonly _addRole = 'Role';
  private readonly _addCategory = 'Category';
  private readonly _addSupplier = 'Supplier';
  private readonly _addProductWithImage = 'Product/add-with-images';

  constructor(private readonly http: HttpClient) {}

  // ========== READ ==========
  getAllProduct(): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getAllProducts}`);
  }

  getAllRole(): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getAllRoles}`);
  }

  getAllCategory(): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getAllCategory}`);
  }

  getSubCategoriesByCategory(categoryId: number): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getSubCategoryByCategory}${categoryId}`);
  }

  getAllSuppliers(): Observable<any> {
    return this.http.get(`${this._baseUrl}${this._getAllSuppliers}`);
  }

  // ========== CREATE ==========
  addRole(formData: any): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addRole}`, formData);
  }

  addCategory(formData: any): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addCategory}`, formData);
  }

  addSupplier(formData: any): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addSupplier}`, formData);
  }

  addProduct(formData: FormData): Observable<any> {
    return this.http.post(`${this._baseUrl}${this._addProductWithImage}`, formData);
  }

  // ========== UPDATE ==========
  updateCategory(id: number, data: any): Observable<any> {
    return this.http.put(`${this._baseUrl}Category`, { id, ...data });
  }

  updateRole(data: any): Observable<any> {
    return this.http.put(`${this._baseUrl}Role`, data);
  }

  updateSupplier(data: any): Observable<any> {
    return this.http.put(`${this._baseUrl}Supplier`, data);
  }

  updateProduct(data: any): Observable<any> {
    return this.http.put(`${this._baseUrl}Product`, data);
  }

  // ========== DELETE ==========
  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Category/${id}`);
  }

  deleteRole(id: string): Observable<any> {
    return this.http.delete(`${this._baseUrl}Role/${id}`);
  }

  deleteSupplier(id: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Supplier/${id}`);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Product/${id}`);
  }

  // ========== ORDERS ==========
  getAllOrders(): Observable<any> {
    return this.http.get(`${this._baseUrl}Order`);
  }

  updateOrderStatus(orderId: number, deliveryStatus: string): Observable<any> {
    return this.http.put(`${this._baseUrl}Order/${orderId}/status`, { DeliveryStatus: deliveryStatus });
  }

  deleteOrder(orderId: number): Observable<any> {
    return this.http.delete(`${this._baseUrl}Order/${orderId}`);
  }

  // ========== USERS ==========
  getAllUsers(): Observable<any> {
    return this.http.get(`${this._baseUrl}Auth/users`);
  }

  // ========== PAYMENTS ==========
  getAllPayments(): Observable<any> {
    return this.http.get(`${this._baseUrl}Payment`);
  }
}