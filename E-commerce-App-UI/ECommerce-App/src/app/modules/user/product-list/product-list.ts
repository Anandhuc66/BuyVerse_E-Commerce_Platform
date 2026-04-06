import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Service } from '../service';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { Unauthorized } from '../../../components/unauthorized/unauthorized';
import { environment } from '../../../Environment/environment';

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.html',
  styleUrls: ['./product-list.css'],
})
export class ProductList implements OnInit {
  
  products: any[] = [];          // filtered products
  allProducts: any[] = [];       // original API data
  loading = true;

  // Filters
  brands: string[] = [];
  selectedBrands: string[] = [];

  minPrice: number = 0;
  maxPrice: number = 200000;
  selectedMinPrice: number = 0;
  selectedMaxPrice: number = 200000;

  sortOrder: string = "relevance";

  constructor(private readonly route: ActivatedRoute, private readonly userService: Service, private readonly toastr: ToastrService, private readonly router: Router, private readonly dialog: MatDialog) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
    const sub = params['subCategoryId'];
    const search = params['search'];

    if (search) {
      this.searchProducts(search);
      return;
    }

    if (sub) { 
      this.loadProducts(sub);
      return;
    }
    });
  }


  loadProducts(subCatId: number) {
    this.userService.getProductsBySubCategory(subCatId).subscribe({
      next: (res) => {
        const data = res.response || [];

        // Convert API to UI format
        this.allProducts = data.map((p: any) => ({
          id: p.id,
          name: p.name,
          price: p.price,
          category: p.subCategoryName,
          brand: p.supplierName,
          image: p.imageUrls?.length ? `${environment.assetUrl}${p.imageUrls[0]}` : 'assets/no-image.png'
        }));

        // Extract brand list
        this.brands = [...new Set(this.allProducts.map(p => p.brand))];

        // set price range dynamically
        const prices = this.allProducts.map(p => p.price);
        this.minPrice = Math.min(...prices);
        this.maxPrice = Math.max(...prices);
        this.selectedMinPrice = this.minPrice;
        this.selectedMaxPrice = this.maxPrice;

        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  // MAIN FILTER FUNCTION
  applyFilters() {
    let filtered = [...this.allProducts];

    // Filter by brand
    if (this.selectedBrands.length > 0) {
      filtered = filtered.filter(p => this.selectedBrands.includes(p.brand));
    }

    // Filter by price
    filtered = filtered.filter(
      p => p.price >= this.selectedMinPrice && p.price <= this.selectedMaxPrice
    );

    // Sorting
    if (this.sortOrder === "lowToHigh") {
      filtered.sort((a, b) => a.price - b.price);
    } else if (this.sortOrder === "highToLow") {
      filtered.sort((a, b) => b.price - a.price);
    }

    this.products = filtered;
  }

  // Brand checkbox action
  toggleBrand(brand: string) {
    if (this.selectedBrands.includes(brand)) {
      this.selectedBrands = this.selectedBrands.filter(b => b !== brand);
    } else {
      this.selectedBrands.push(brand);
    }
    this.applyFilters();
  }

  // Price slider events
  updatePriceFilter() {
    this.applyFilters();
  }

  // Sorting
  changeSorting(order: string) {
  this.sortOrder = order;
  this.applyFilters();
}

  trackByProduct(index: number, item: any) {
    return item.id;
  }

  searchProducts(keyword: string) {
  this.userService.searchProducts(keyword).subscribe({
    next: (res) => {
      const data = res.response || [];

      this.allProducts = data.map((p: any) => ({
        id: p.id,
        name: p.name,
        price: p.price,
        category: p.subCategoryName,
        brand: p.supplierName,
          image: p.imageUrls?.length ? `${environment.assetUrl}${p.imageUrls[0]}` : 'assets/no-image.png'
      }));

      // Extract brands dynamically
      this.brands = [...new Set(this.allProducts.map(p => p.brand))];

      // Set price limits dynamically
      const prices = this.allProducts.map(p => p.price);
      this.minPrice = Math.min(...prices);
      this.maxPrice = Math.max(...prices);
      this.selectedMinPrice = this.minPrice;
      this.selectedMaxPrice = this.maxPrice;

      this.applyFilters();
      this.loading = false;
    },
    error: (err) => {
      console.error('Search error:', err);
      this.loading = false;
    }
  });
}

// call to add to cart
addToCart(product: any): void {
  const userId = localStorage.getItem('userId');

  if (!userId) {
    this.dialog.open(Unauthorized, {
      width: '450px',
      panelClass: 'auth-dialog'
    });
    return;
  }

  const payload = {
    userId,
    productId: product.id,
    quantity: 1   // product list always adds 1
  };

  this.userService.addToCart(payload).subscribe({
    next: (res: any) => {

      // Backend sends 200 with isError=true for duplicates
      if (res?.isError && res?.errors?.length > 0) {
        const msg = res.errors[0].errorMessage;
        this.toastr.warning(msg);
        return;
      }

      this.toastr.success("Added to cart successfully!");
      this.userService.increaseCartCount();
    },
    error: (err) => {
      const msg = err.error?.errors?.[0]?.errorMessage || "Error adding product";
      this.toastr.error(msg);
    }
  });
}
viewDetails(product: any) {
  this.router.navigate(['/product', product.id]);
}

}
