import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Service } from '../service';
import { forkJoin, catchError, of } from 'rxjs';
import { Chart, ChartConfiguration, ChartData, registerables } from 'chart.js';
import { environment } from '../../../Environment/environment';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  // Stats
  totalProducts = 0;
  totalOrders = 0;
  totalRevenue = 0;
  totalUnitsSold = 0;

  // Delivery status breakdown
  pendingDelivery = 0;
  processingOrders = 0;
  shippedOrders = 0;
  deliveredOrders = 0;
  cancelledOrders = 0;

  // Payment status breakdown
  paidOrders = 0;
  codOrders = 0;
  pendingPayment = 0;

  // Recent orders
  recentOrders: any[] = [];

  // Top products
  topProducts: any[] = [];

  isLoading = true;
  today = new Date();

  // Date range filter
  startDate = '';
  endDate = '';

  // Raw data for re-filtering
  private allOrders: any[] = [];
  private allProducts: any[] = [];

  // ===== REVENUE LINE CHART =====
  revenueChartData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        label: 'Revenue',
        data: [],
        borderColor: '#00897B',
        backgroundColor: 'rgba(0,137,123,0.1)',
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#00897B',
        pointRadius: 4,
        pointHoverRadius: 6
      },
      {
        label: 'Orders',
        data: [],
        borderColor: '#ffb74d',
        backgroundColor: 'transparent',
        borderDash: [6, 4],
        tension: 0.4,
        pointBackgroundColor: '#ffb74d',
        pointRadius: 3,
        pointHoverRadius: 5
      }
    ]
  };
  revenueChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: true, position: 'top', labels: { color: '#64748B', usePointStyle: true, padding: 20 } },
      tooltip: {
        backgroundColor: '#1E293B',
        titleColor: '#fff',
        bodyColor: '#CBD5E1',
        padding: 12,
        cornerRadius: 8,
        callbacks: {
          label: (ctx: any) => ctx.dataset.label === 'Revenue' ? `₹${ctx.parsed.y.toLocaleString()}` : `${ctx.parsed.y} orders`
        }
      }
    },
    scales: {
      x: { ticks: { color: '#64748B' }, grid: { color: 'rgba(0,0,0,0.06)' } },
      y: { ticks: { color: '#64748B', callback: (v: any) => v >= 1000 ? '₹' + (v / 1000).toFixed(1) + 'K' : '₹' + v }, grid: { color: 'rgba(0,0,0,0.06)' } }
    }
  };

  // ===== DELIVERY DOUGHNUT =====
  deliveryDoughnutData: ChartData<'doughnut'> = {
    labels: ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'],
    datasets: [{
      data: [0, 0, 0, 0, 0],
      backgroundColor: ['#ffb74d', '#ff9800', '#42a5f5', '#66bb6a', '#ef5350'],
      borderWidth: 0,
      hoverOffset: 8
    }]
  };
  deliveryDoughnutOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '70%',
    plugins: {
      legend: { display: false },
      tooltip: { backgroundColor: '#1E293B', titleColor: '#fff', bodyColor: '#CBD5E1', padding: 10, cornerRadius: 8 }
    }
  };

  constructor(private readonly supplierService: Service, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    const supplierId = localStorage.getItem('supplierId');
    if (!supplierId) {
      this.isLoading = false;
      return;
    }

    forkJoin({
      orders: this.supplierService.getSupplierOrders().pipe(catchError(() => of({ response: [] }))),
      products: this.supplierService.getProductsBySupplier(+supplierId).pipe(catchError(() => of({ response: [] })))
    }).subscribe({
      next: (data: any) => {
        this.allOrders = data.orders?.response || [];
        this.allProducts = data.products?.response || [];

        this.totalProducts = this.allProducts.length;

        this.isLoading = false;
        this.processFilteredData();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onDateRangeChange(): void {
    this.processFilteredData();
  }

  private processFilteredData(): void {
    const orders = this.filterOrdersByDate(this.allOrders);

    this.totalOrders = orders.length;
    this.resetStats();
    this.computeStats(orders);
    this.computeTopProducts(orders, this.allProducts);

    this.recentOrders = [...orders]
      .sort((a: any, b: any) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
      .slice(0, 8);

    this.cdr.markForCheck();
    setTimeout(() => {
      this.buildRevenueChart(orders);
      this.buildDeliveryDoughnut();
      this.cdr.markForCheck();
    }, 50);
  }

  private filterOrdersByDate(orders: any[]): any[] {
    const start = this.startDate ? new Date(this.startDate + 'T00:00:00') : null;
    const end = this.endDate ? new Date(this.endDate + 'T23:59:59') : null;
    if (!start && !end) return orders;
    return orders.filter((o: any) => {
      const d = new Date(o.orderDate);
      if (start && d < start) return false;
      if (end && d > end) return false;
      return true;
    });
  }

  private resetStats(): void {
    this.totalRevenue = 0;
    this.totalUnitsSold = 0;
    this.pendingDelivery = 0;
    this.processingOrders = 0;
    this.shippedOrders = 0;
    this.deliveredOrders = 0;
    this.cancelledOrders = 0;
    this.paidOrders = 0;
    this.codOrders = 0;
    this.pendingPayment = 0;
  }

  computeStats(orders: any[]): void {
    for (const order of orders) {
      // Calculate revenue from supplier's products only
      const orderProducts = order.products || [];
      for (const p of orderProducts) {
        this.totalRevenue += (p.unitPrice || 0) * (p.quantity || 0);
        this.totalUnitsSold += p.quantity || 0;
      }

      // Delivery status
      const ds = (order.deliveryStatus || '').toLowerCase();
      if (ds === 'pending') this.pendingDelivery++;
      else if (ds === 'processing') this.processingOrders++;
      else if (ds === 'shipped') this.shippedOrders++;
      else if (ds === 'delivered') this.deliveredOrders++;
      else if (ds === 'cancelled') this.cancelledOrders++;

      // Payment status
      const ps = (order.status || '').toLowerCase();
      if (ps === 'paid') this.paidOrders++;
      else if (ps === 'cod') this.codOrders++;
      else if (ps === 'pending') this.pendingPayment++;
    }
  }

  computeTopProducts(orders: any[], products: any[]): void {
    const productSold: Record<number, number> = {};
    const productRevenue: Record<number, number> = {};
    for (const order of orders) {
      for (const d of (order.products || [])) {
        productSold[d.productId] = (productSold[d.productId] || 0) + (d.quantity || 1);
        productRevenue[d.productId] = (productRevenue[d.productId] || 0) + (d.unitPrice || 0) * (d.quantity || 1);
      }
    }
    const productMap = new Map(products.map((p: any) => [p.id, p]));
    const imageBase = environment.baseUrl.replace('/api/', '/');
    this.topProducts = Object.entries(productSold)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([id, qty]) => {
        const prod = productMap.get(+id);
        const firstImage = prod?.imageUrls?.length ? imageBase + prod.imageUrls[0] : null;
        return {
          name: prod?.name || `Product #${id}`,
          image: firstImage,
          sold: qty,
          price: prod?.price || 0,
          revenue: productRevenue[+id] || 0
        };
      });
  }

  private toLocalDateKey(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  buildRevenueChart(orders: any[]): void {
    const dayMap: Record<string, { revenue: number; count: number }> = {};
    const now = new Date();

    // Find the earliest order date, default to 30 days ago
    let earliest = new Date(now);
    earliest.setDate(earliest.getDate() - 29);
    if (orders.length > 0) {
      const dates = orders.map((o: any) => new Date(o.orderDate).getTime());
      const minDate = new Date(Math.min(...dates));
      if (minDate < earliest) earliest = minDate;
    }

    // Build day buckets from earliest to today
    const d = new Date(earliest);
    while (d <= now) {
      dayMap[this.toLocalDateKey(d)] = { revenue: 0, count: 0 };
      d.setDate(d.getDate() + 1);
    }
    for (const order of orders) {
      const key = this.toLocalDateKey(new Date(order.orderDate));
      if (dayMap[key]) {
        const orderRev = (order.products || []).reduce((s: number, p: any) => s + (p.unitPrice || 0) * (p.quantity || 0), 0);
        dayMap[key].revenue += orderRev;
        dayMap[key].count += 1;
      }
    }
    const labels = Object.keys(dayMap).map(k => {
      const parts = k.split('-');
      const d = new Date(+parts[0], +parts[1] - 1, +parts[2]);
      return d.toLocaleDateString('en-US', { day: 'numeric', month: 'short' });
    });
    this.revenueChartData = {
      labels,
      datasets: [
        { ...this.revenueChartData.datasets[0], data: Object.values(dayMap).map(v => v.revenue) },
        { ...this.revenueChartData.datasets[1], data: Object.values(dayMap).map(v => v.count) }
      ]
    };
  }

  buildDeliveryDoughnut(): void {
    this.deliveryDoughnutData = {
      ...this.deliveryDoughnutData,
      datasets: [{
        ...this.deliveryDoughnutData.datasets[0],
        data: [this.pendingDelivery, this.processingOrders, this.shippedOrders, this.deliveredOrders, this.cancelledOrders]
      }]
    };
  }

  getDeliveryStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'delivered': return 'badge-delivered';
      case 'shipped': return 'badge-shipped';
      case 'processing': return 'badge-processing';
      case 'pending': return 'badge-pending';
      case 'cancelled': return 'badge-cancelled';
      default: return 'badge-default';
    }
  }

  getPaymentStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'paid': return 'badge-paid';
      case 'cod': return 'badge-cod';
      case 'pending': return 'badge-pending';
      default: return 'badge-default';
    }
  }

  getOrderPercentage(count: number): number {
    return this.totalOrders > 0 ? Math.round((count / this.totalOrders) * 100) : 0;
  }

  getOrderRevenue(order: any): number {
    return (order.products || []).reduce((s: number, p: any) => s + (p.unitPrice || 0) * (p.quantity || 0), 0);
  }
}
