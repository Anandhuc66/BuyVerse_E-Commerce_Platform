import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  trigger,
  transition,
  style,
  animate,
} from '@angular/animations';
import { AuthService } from '../../service/auth.service';
import { Subject, Subscription, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { Service } from '../../../modules/user/service';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrl: './header.css',
  animations: [
    trigger('menuSlide', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('250ms ease-out', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ height: 0, opacity: 0 })),
      ]),
    ]),
  ],
})
export class Header implements OnInit, OnDestroy{
 isMobileOpen = false;
  isShrunk = false;
  isLoggedIn = false;
  private authSub!: Subscription;
  cartCount = 0;

  searchQuery: string = "";
  suggestions: string[] = [];
  showSuggestions = false;
  loading = false;
  private searchSubject = new Subject<string>();
  private searchSub!: Subscription;

  constructor(private readonly router: Router, private readonly auth: AuthService, private readonly cartService: Service) {}

  ngOnInit() {
    // Subscribe to Auth changes
    this.authSub = this.auth.authStatus$.subscribe(status => {
      this.isLoggedIn = status;
    });

    // Initial check on page load
    this.isLoggedIn = this.auth.isLoggedIn();

    this.cartService.cartCount$.subscribe(count => {
    this.cartCount = count;
  });

    // Setup search pipeline: debounce + cancel previous requests
    this.searchSub = this.searchSubject.pipe(
      debounceTime(150),
      distinctUntilChanged(),
      switchMap(keyword => {
        if (keyword.length < 2) {
          this.suggestions = [];
          this.showSuggestions = false;
          this.loading = false;
          return of(null);
        }
        this.loading = true;
        return this.cartService.searchProducts(keyword).pipe(
          catchError(() => { this.loading = false; return of(null); })
        );
      })
    ).subscribe(res => {
      if (res) {
        const list = res.response || [];
        this.suggestions = [...new Set(list.filter((p: any) => typeof p.name === 'string').map((p: any) => p.name))] as string[];
        this.loading = false;
      }
    });
  }

  ngOnDestroy() {
    this.authSub?.unsubscribe();
    this.searchSub?.unsubscribe();
  }

logout() {
  const dropdown = document.querySelector('.dropdown-toggle.show') as HTMLElement;
  if (dropdown) dropdown.click(); // close it if open

  this.auth.logout();
  this.isLoggedIn = false;
  this.router.navigate(['/login']);
}

  @HostListener('window:scroll')
  onScroll() {
    this.isShrunk = window.scrollY > 50;
  }

animateCart(event: Event) {
  const icon = (event.currentTarget as HTMLElement).querySelector('.cart-icon');
  if (icon) {
    icon.classList.add('cart-bounce');
    setTimeout(() => icon.classList.remove('cart-bounce'), 500);
  }
}


onSearch(event: Event) {
  event.preventDefault();
  const keyword = this.searchQuery.trim();
  if (!keyword) return;

  this.router.navigate(['/product-list'], {
    queryParams: { search: keyword }
  });

  this.showSuggestions = false;
}


onTyping() {
  if (!this.searchQuery.trim()) {
    this.suggestions = [];
    this.showSuggestions = false;
    return;
  }

  this.showSuggestions = true;
  this.loading = true;
  this.searchSubject.next(this.searchQuery.trim());
}


selectSuggestion(text: string) {
  this.searchQuery = text;
  this.showSuggestions = false;

  this.router.navigate(['/product-list'], {
  queryParams: { search: text }
});

}

@HostListener('document:click', ['$event'])
onClickOutside(event: Event) {
  const target = event.target as HTMLElement;

  const clickedInside = target.closest('.search-wrapper');
  if (!clickedInside) {
    this.showSuggestions = false;
  }
}
onBlur() {
  setTimeout(() => {
    this.showSuggestions = false;
  }, 150);
}


}
