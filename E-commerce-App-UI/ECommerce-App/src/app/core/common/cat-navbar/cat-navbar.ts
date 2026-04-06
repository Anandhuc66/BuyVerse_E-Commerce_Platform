import { Component, OnInit } from '@angular/core';
import { Service } from '../../../modules/admin/service';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-cat-navbar',
  standalone: false,
  templateUrl: './cat-navbar.html',
  styleUrls: ['./cat-navbar.css']
})
export class CatNavbar implements OnInit {
  
  categories: any[] = [];   // Dynamic categories from API
  activeCategory: any = null;

  constructor(private readonly api: Service, private readonly router: Router) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  // Load Categories + SubCategories
  loadCategories() {
    this.api.getAllCategory().subscribe({
      next: (res) => {
        const cats = res.response || [];
        if (cats.length === 0) {
          this.categories = [];
          return;
        }

        const subCategoryRequests = cats.map((cat: any) =>
          this.api.getSubCategoriesByCategory(cat.id).pipe(
            catchError(() => of({ response: [] }))
          )
        );

        forkJoin(subCategoryRequests).subscribe((results) => {
          const resultsArr = results as any[];
          cats.forEach((cat: any, index: number) => {
            cat.subcategories = resultsArr[index]?.response?.map((sc: any) => ({
              id: sc.id,
              title: sc.name
            })) || [];
          });
          this.categories = cats;
        });
      },
      error: () => {
        this.categories = [];
      }
    });
  }

  trackByCategory(index: number, cat: any): number {
    return cat.id;
  }

  trackBySub(index: number, sub: any): number {
    return sub.id;
  }
  openDropdown(cat: any) {
    this.activeCategory = cat;
  }

  closeDropdown() {
    this.activeCategory = null;
  }
  
navigateToSubCategory(subCategoryId: number) {
  this.router.navigate(
    ['/product-list'],
    { queryParams: { subCategoryId } }
  );
}
}
