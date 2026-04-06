import { Injectable } from '@angular/core';
import { environment } from '../../Environment/environment';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, switchMap, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly _baseUrl = environment.baseUrl;
  private readonly _userRegister = 'Auth/register-user';
  private readonly _supplierRegister = 'Auth/register-supplier';
  private readonly _allLogin = 'Auth/login';
  private readonly _getSupplierByuserId='Supplier/GetSupplierByUserId/'

  // ✅ Initialize BehaviorSubject from localStorage
  private readonly authStatus = new BehaviorSubject<boolean>(this.hasToken());
  authStatus$ = this.authStatus.asObservable();

  constructor(private readonly http: HttpClient) {}

  // ✅ Helper to safely check token existence
  private hasToken(): boolean {
    const t = localStorage.getItem('token');
    return !!t && t !== 'undefined' && t !== 'null';
  }

  // 🧾 Register User
  registerUser(data: any): Observable<any> {
    return this.http.post(this._baseUrl + this._userRegister, data);
  }

  // 🧾 Register Supplier
  registerSupplier(data: any): Observable<any> {
    return this.http.post(this._baseUrl + this._supplierRegister, data);
  }

  // 🔐 Login
allLogin(data: any): Observable<any> {
  return this.http.post(this._baseUrl + this._allLogin, data).pipe(
    switchMap((res: any) => {
      const token = res?.response?.token ?? null;
      const role = res?.response?.role ?? null;
      const userId = res?.response?.userId ?? null;
      const email = res?.response?.email ?? null;
      const fullName = res?.response?.fullName ?? null;
      const gender = res?.response?.gender ?? null;
      const phoneNumber = res?.response?.phoneNumber ?? null;

      if (token) {
        localStorage.setItem('token', token);
        if (role) localStorage.setItem('role', role);
        if (userId) localStorage.setItem('userId', userId);
        if (email) localStorage.setItem('email', email);
        if (fullName) localStorage.setItem('fullName', fullName);
        if (gender) localStorage.setItem('gender', gender);
        if (phoneNumber) localStorage.setItem('phoneNumber', phoneNumber);

        // ✅ Fetch SupplierId before completing login if Supplier
        if (role === 'Supplier' && userId) {
          const supplierUrl = `${this._baseUrl}${this._getSupplierByuserId}${userId}`;
          return this.http.get(supplierUrl).pipe(
            tap((supplier: any) => {
              const supplierId = supplier?.response?.supplierId ?? supplier?.supplierId;
              if (supplierId) {
                localStorage.setItem('supplierId', supplierId.toString());
                console.log('✅ SupplierId stored:', supplierId);
              } else {
                console.warn('⚠️ Supplier ID not found in response', supplier);
              }
            }),
            switchMap(() => {
              this.authStatus.next(true);
              return of(res);
            })
          );
        }

        // ✅ Notify authentication change
        this.authStatus.next(true);
      }
      return of(res);
    })
  );
}


  // 🧭 Get role
  getUserRole(): string | null {
    return localStorage.getItem('role');
  }

  // Logout
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');
    localStorage.removeItem('email');
    localStorage.removeItem('fullName');
    localStorage.removeItem('gender');
    localStorage.removeItem('phoneNumber');
    localStorage.removeItem('supplierId');

    // Notify logout
    this.authStatus.next(false);
  }

  // Check if logged in (with JWT expiry check)
  isLoggedIn(): boolean {
    const token = localStorage.getItem('token');
    if (!token || token === 'undefined' || token === 'null' || token.trim() === '') {
      return false;
    }
    // Decode JWT and check expiry
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (payload.exp && (payload.exp * 1000) < Date.now()) {
        this.logout();
        return false;
      }
    } catch {
      return false;
    }
    return true;
  }


  // 🔹 Get stored token
  getToken(): string | null {
    return localStorage.getItem('token');
  }
}
