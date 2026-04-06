import { 
  Component, 
  ChangeDetectorRef, 
  ChangeDetectionStrategy, 
  OnInit, 
  OnDestroy,
  ViewChild,
  ElementRef 
} from '@angular/core';
import { environment } from '../../../Environment/environment';
import {
  trigger,
  style,
  transition,
  animate,
} from '@angular/animations';
import { Service } from '../service';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  categoryName: string;
  imageUrls: string[];   // <--- multiple images
}



@Component({
  selector: 'app-homepage1',
  standalone: false,
  templateUrl: './homepage1.html',
  styleUrls: ['./homepage1.css'],
  changeDetection: ChangeDetectionStrategy.Default, // âœ… ensure view updates properly
  animations: [
        trigger('fadeAnimation', [
          transition(':enter', [
            style({
              opacity: 0,
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              height: '100%',
            }),
            animate('1000ms ease-in', style({ opacity: 1 })),
          ]),
          transition(':leave', [
            style({
              opacity: 1,
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              height: '100%',
            }),
            animate('1000ms ease-out', style({ opacity: 0 })),
         ]),
    ]),
  ],
})
export class Homepage1 implements OnInit, OnDestroy {
  hoveredId: number | null = null;
  hoveredCategory: any = null;
  baseUrl = environment.assetUrl;

  /** Trending auto-scroll */
  trendingProducts: Product[] = [];
  private trendingIntervalId: any;
  private trendingRefreshId: any;


features = [
  { icon: 'bi bi-truck', title: 'Free Delivery', subtitle: 'Free Shipping on all order' },
  { icon: 'bi bi-arrow-repeat', title: 'Return Policy', subtitle: 'Free Shipping on all order' },
  { icon: 'bi bi-headset', title: '24/7 Support', subtitle: 'Free Shipping on all order' },
  { icon: 'bi bi-credit-card', title: 'Secure Payment', subtitle: 'Free Shipping on all order' },
];

  products: Product[] = [];

  slides = [
  {
    image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=1300&h=350&fit=crop&auto=format',
    alt: 'Electronics Sale',
    title: 'Top Electronics Deals',
    subtitle: 'Up to 40% off on the latest gadgets and accessories.',
    cta: 'Shop Electronics',
  },
  {
    image: 'https://images.unsplash.com/photo-1512436991641-6745cdb1723f?w=1300&h=350&fit=crop&auto=format',
    alt: 'Fashion Collection',
    title: 'New Fashion Arrivals',
    subtitle: 'Style up with our trendy menâ€™s and womenâ€™s wear.',
    cta: 'Explore Fashion',
  },
  {
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=1300&h=350&fit=crop&auto=format',
    alt: 'Home & Furniture',
    title: 'Beautify Your Home',
    subtitle: 'Furniture, dÃ©cor & more â€” everything to upgrade your home.',
    cta: 'Discover Now',
  }
];


  currentSlide = 0;
  private intervalId: any;
  loading = true;

  constructor(private readonly cdr: ChangeDetectorRef, private readonly userService: Service) {}

  ngOnInit(): void {
    this.startAutoSlide();
    this.loadProducts();
  }
loadProducts() {
  this.userService.getAllProduct().subscribe({
    next: (res) => {
      console.log("API Response:", res);

      this.products = [...res.response].sort(() => Math.random() - 0.5);
      this.shuffleTrending();
      this.startTrendingAutoScroll();
      this.startTrendingRefresh();

      this.loading = false;
      this.cdr.detectChanges();
    },
    error: (err) => {
      console.error("Error fetching products:", err);
      this.loading = false;
    }
  });
}

  /** Trending — shuffle products for the auto-scroll strip */
  @ViewChild('trendingTrack') trendingTrackRef!: ElementRef<HTMLDivElement>;

  shuffleTrending() {
    const shuffled = [...this.products].sort(() => Math.random() - 0.5);
    this.trendingProducts = shuffled.slice(0, 12);
  }

  startTrendingAutoScroll() {
    this.stopTrendingAutoScroll();
    this.trendingIntervalId = setInterval(() => {
      const el = this.trendingTrackRef?.nativeElement;
      if (!el) return;
      const maxScroll = el.scrollWidth - el.clientWidth;
      if (el.scrollLeft >= maxScroll - 2) {
        el.scrollTo({ left: 0, behavior: 'smooth' });
      } else {
        el.scrollBy({ left: 220, behavior: 'smooth' });
      }
    }, 3000);
  }

  stopTrendingAutoScroll() {
    if (this.trendingIntervalId) {
      clearInterval(this.trendingIntervalId);
      this.trendingIntervalId = null;
    }
  }

  pauseTrendingScroll() {
    this.stopTrendingAutoScroll();
  }

  resumeTrendingScroll() {
    this.startTrendingAutoScroll();
  }

  scrollTrending(direction: 'left' | 'right') {
    const el = this.trendingTrackRef?.nativeElement;
    if (!el) return;
    const amount = direction === 'left' ? -300 : 300;
    el.scrollBy({ left: amount, behavior: 'smooth' });
  }

  /** Auto-refresh: shuffle trending products every 30s */
  startTrendingRefresh() {
    this.stopTrendingRefresh();
    this.trendingRefreshId = setInterval(() => {
      this.shuffleTrending();
      this.cdr.detectChanges();
      const el = this.trendingTrackRef?.nativeElement;
      if (el) el.scrollTo({ left: 0, behavior: 'smooth' });
    }, 30000);
  }

  stopTrendingRefresh() {
    if (this.trendingRefreshId) {
      clearInterval(this.trendingRefreshId);
      this.trendingRefreshId = null;
    }
  }




   /** TrackBy function for ngFor performance */
  trackByProduct(index: number, product: Product) {
    return product.id;
  }

  /** ðŸ” Auto slide */
  startAutoSlide() {
    this.stopAutoSlide();
    this.intervalId = setInterval(() => {
      this.nextSlide();
      this.cdr.markForCheck(); // âœ… Ensures Angular detects the change
    }, 4000);
  }

  nextSlide() {
    this.currentSlide = (this.currentSlide + 1) % this.slides.length;
  }

  prevSlide() {
    this.currentSlide =
      (this.currentSlide - 1 + this.slides.length) % this.slides.length;
  }

  goToSlide(index: number) {
    this.currentSlide = index;
    this.restartAutoSlide();
  }

  restartAutoSlide() {
    this.stopAutoSlide();
    this.startAutoSlide();
  }

  stopAutoSlide() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  ngOnDestroy(): void {
    this.stopAutoSlide();
    this.stopTrendingAutoScroll();
    this.stopTrendingRefresh();
  }
  

}
