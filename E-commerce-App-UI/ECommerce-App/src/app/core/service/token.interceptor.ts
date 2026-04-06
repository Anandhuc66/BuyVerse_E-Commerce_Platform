import { Injectable } from '@angular/core';
import { AuthService } from '../service/auth.service';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class TokenInterceptor implements HttpInterceptor {
  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();

    let cloned = req;
    if (token) {
      cloned = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }

    return next.handle(cloned).pipe(
      catchError((error: HttpErrorResponse) => {
        // Only redirect to login if user was previously logged in (expired token)
        // Don't redirect guests — let the component handle 401 (e.g. show login popup)
        if (error.status === 401 && token) {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
