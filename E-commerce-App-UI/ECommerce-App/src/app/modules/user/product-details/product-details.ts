  import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
  import { ActivatedRoute, Router } from '@angular/router';
  import {
    trigger,
    transition,
    style,
    animate,
  } from '@angular/animations';
import { Service } from '../service';
import { MatDialog } from '@angular/material/dialog';
import { Unauthorized } from '../../../components/unauthorized/unauthorized';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../../Environment/environment';


  interface Product {
    id: number;
    title: string;
    price: number;
    description: string;
    image: string;
    rating: number;
    category: string;
    images: string[];
    subCategoryId?: number;
  }

  @Component({
    selector: 'app-product-details',
    standalone: false,
    templateUrl: './product-details.html',
    styleUrl: './product-details.css',
    animations: [
      trigger('fadeIn', [
        transition(':enter', [
          style({ opacity: 0, transform: 'translateY(10px)' }),
          animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
        ]),
      ]),
    ],
  })
  export class ProductDetails implements OnInit {
    product!: Product;
    selectedImage = '';
    quantity = 1;
    loading = true;
    similarProducts: Product[] = [];
    baseUrl = environment.assetUrl;
    reviewForm!: FormGroup;
    reviews: any[] = [];   // Store loaded reviews
    avgRating: number = 0;
    totalRatings: number = 0;
   
    constructor(
      private readonly route: ActivatedRoute,
      private readonly router: Router,
      private readonly userService: Service,
      private readonly dialog: MatDialog,
      private readonly toastr: ToastrService,
      private readonly fb: FormBuilder,
      private readonly cdr: ChangeDetectorRef
    ) {}

    

ngOnInit(): void {
  this.initReviewForm();

  // Subscribe to route param changes so clicking similar products reloads data
  this.route.paramMap.subscribe(params => {
    const id = Number(params.get('id'));
    if (id) {
      this.loading = true;
      this.quantity = 1;
      this.similarProducts = [];
      this.reviews = [];
      this.loadProductById(id);
      this.loadReviews(id);
    }
  });
}

initReviewForm() {
  this.reviewForm = this.fb.group({
    productId: 0,
    userId: '',
    rating: [0, Validators.required],
    title: ['', Validators.required],
    comment: ['', Validators.required],
  });
}

loadProductById(id: number) {
  this.userService.getProductById(id).subscribe({
    next: (res: any) => {

      if (!res?.response) {
        console.error("Invalid API response:", res);
        return;
      }

      const p = res.response; // shortcut

      const images = p.imageUrls?.length ? p.imageUrls : [];

      this.product = {
        id: p.id,
        title: p.name,                     // FIXED
        price: p.price,
        description: p.description,
        image: images.length ? this.baseUrl + images[0] : 'assets/no-image.png',
        rating: 4.5,
        category: p.categoryName,              // FIXED
        images: images.map((url: string) => this.baseUrl + url), // FIXED
        subCategoryId: p.subCategoryId
      };

      this.selectedImage = this.product.images.length ? this.product.images[0] : 'assets/no-image.png';
      this.loading = false;
      this.cdr.detectChanges();
      if (this.product.subCategoryId) {
        this.loadSimilarProducts(this.product.subCategoryId, this.product.id);
      }
    },
    error: (err) => {
      console.error('Error loading product', err);
      this.loading = false;
      this.cdr.detectChanges();
    }
  });
}



    loadSimilarProducts(subCategoryId: number, currentProductId: number) {
      this.userService.getProductsBySubCategory(subCategoryId).subscribe({
        next: (res: any) => {
          const products = res?.response || [];
          this.similarProducts = products
            .filter((p: any) => p.id !== currentProductId)
            .slice(0, 4)
            .map((p: any) => ({
              id: p.id,
              title: p.name,
              price: p.price,
              description: p.description,
              image: p.imageUrls?.length ? this.baseUrl + p.imageUrls[0] : 'assets/no-image.png',
              rating: 4.5,
              category: p.categoryName,
              images: p.imageUrls?.map((url: string) => this.baseUrl + url) || []
            }));
        },
        error: () => this.similarProducts = []
      });
    }

    changeImage(img: string) {
      this.selectedImage = img;
    }

    incrementQty() {
      this.quantity++;
    }

    decrementQty() {
      if (this.quantity > 1) this.quantity--;
    }

    // addToCart() {
    //   alert(`Added ${this.quantity} × ${this.product.title} to cart!`);
    // }

    // BuyNow() {
    //   this.router.navigate(['/user/checkout']);
    // }
    BuyNow() {
    const isLoggedIn = localStorage.getItem("token");

    if (!isLoggedIn) {
      this.dialog.open(Unauthorized, {
        width: "450px",
        panelClass: "auth-dialog"
      });
      return;
    }

  // If logged in → redirect to payment
  console.log("Proceed to checkout");
  this.router.navigate(
    ['/checkout'],
    {
      state: {
        product: {
          id: this.product.id,
          title: this.product.title,
          price: this.product.price,
          images: this.product.images,
          quantity: this.quantity   // 👈 send selected qty
        }
      }
    }
  );
}

// call to add to cart
addToCart(): void {
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
    productId: this.product.id,
    quantity: this.quantity
  };

  this.userService.addToCart(payload).subscribe({
    next: (res: any) => {

      // 🟥 BACKEND RETURNS 200 OK WITH isError=true
      if (res?.isError && res?.errors?.length > 0) {
        const msg = res.errors[0].errorMessage;
        this.toastr.warning(msg);
        return;
      }

      // 🟩 SUCCESS
      this.toastr.success("Added to cart successfully!");
      this.userService.increaseCartCount();
    },

    error: (err) => {
      const msg = err.error?.errors?.[0]?.errorMessage || "Error adding product";
      this.toastr.error(msg);
    }
  });
}

  goToProduct(id: number) {
    this.router.navigate(['/product', id]);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  selectRating(rate: number) {
  this.reviewForm.patchValue({ rating: rate });
  }

 loadReviews(productId: number) {
  this.userService.getReviews(productId).subscribe({
    next: (res: any) => {
      this.reviews = res.response || [];
    },
    error: () => this.reviews = []
  });
}


submitReview() {
  const userId = localStorage.getItem('userId');
  if (!userId) {
    this.dialog.open(Unauthorized, {
      width: '450px',
      panelClass: 'auth-dialog'
    });
    return;
  }

  this.reviewForm.patchValue({
    productId: this.product.id,
    userId: userId
  });

  this.userService.submitReview(this.reviewForm.value).subscribe({
    next: () => {
      this.toastr.success("Review submitted!");
      this.reviewForm.reset({ rating: 0 });
      this.loadReviews(this.product.id);
    },
    error: () => this.toastr.error("Error submitting review")
  });
}
showReviewForm: boolean = false;

toggleReviewForm() {
  this.showReviewForm = !this.showReviewForm;
}
getRatingPercent(star: number): number {
  const count = this.getRatingCount(star);
  return this.totalRatings ? (count / this.totalRatings) * 100 : 0;
}

// Count reviews for each star
getRatingCount(star: number): number {
  return this.reviews.filter(r => r.rating === star).length;
}

  trackByImage(index: number, img: any): number {
    return img.id || index;
  }
}
