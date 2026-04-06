import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-main-layout',
  standalone: false,
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayout {
showLayout = true;

  constructor(private readonly router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        const hiddenRoutes = [
          '/login',
          '/register',
          '/admin',
          '/supplier',
          '/supplier-register',
          '/user-dashboard'
        ];
        this.showLayout = !hiddenRoutes.some(path => event.url.startsWith(path));
      });
  }
}
