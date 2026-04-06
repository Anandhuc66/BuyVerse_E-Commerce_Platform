import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Service } from '../service';
import { forkJoin, of, catchError } from 'rxjs';
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
  totalUsers = 0;
  totalSuppliers = 0;
  totalCategories = 0;
  totalRevenue = 0;

  // Order status breakdown - Payment
  paidOrders = 0;
  codOrders = 0;
  pendingPaymentOrders = 0;

  // Order status breakdown - Delivery
  pendingDeliveryOrders = 0;
  processingOrders = 0;
  shippedOrders = 0;
  deliveredOrders = 0;
  cancelledOrders = 0;

  // Recent orders
  recentOrders: any[] = [];

  // Top products (by order frequency)
  topProducts: any[] = [];

  // Category sales breakdown
  categorySales: { name: string; revenue: number }[] = [];

  // Loading
  isLoading = true;

  today = new Date();

  // Date range filter
  startDate = '';
  endDate = '';

  // Raw data for re-filtering
  private allOrders: any[] = [];
  private allProducts: any[] = [];
  private allCategories: any[] = [];

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

  // ===== ORDER STATUS DOUGHNUT (Payment) =====
  orderDoughnutData: ChartData<'doughnut'> = {
    labels: ['Paid', 'COD', 'Pending'],
    datasets: [{
      data: [0, 0, 0],
      backgroundColor: ['#66bb6a', '#42a5f5', '#ffb74d'],
      borderWidth: 0,
      hoverOffset: 8
    }]
  };
  orderDoughnutOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '70%',
    plugins: {
      legend: { display: false },
      tooltip: { backgroundColor: '#1E293B', titleColor: '#fff', bodyColor: '#CBD5E1', padding: 10, cornerRadius: 8 }
    }
  };

  // ===== DELIVERY STATUS DOUGHNUT =====
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

  // ===== CATEGORY DOUGHNUT =====
  categoryChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: ['#ff7043', '#42a5f5', '#ffb74d', '#66bb6a', '#ab47bc', '#ef5350', '#26c6da', '#ec407a'],
      borderWidth: 0,
      hoverOffset: 8
    }]
  };
  categoryChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '65%',
    plugins: {
      legend: { display: false },
      tooltip: { backgroundColor: '#1E293B', titleColor: '#fff', bodyColor: '#CBD5E1', padding: 10, cornerRadius: 8 }
    }
  };

  constructor(private readonly adminService: Service, private readonly cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    forkJoin({
      products: this.adminService.getAllProduct().pipe(catchError(() => of({ response: [] }))),
      orders: this.adminService.getAllOrders().pipe(catchError(() => of({ response: [] }))),
      users: this.adminService.getAllUsers().pipe(catchError(() => of({ response: [] }))),
      suppliers: this.adminService.getAllSuppliers().pipe(catchError(() => of({ response: [] }))),
      categories: this.adminService.getAllCategory().pipe(catchError(() => of({ response: [] })))
    }).subscribe({
      next: (data: any) => {
        try {
        this.allProducts = data.products?.response || [];
        this.allOrders = data.orders?.response || [];
        this.allCategories = data.categories?.response || [];

        this.totalProducts = this.allProducts.length;

        const users = data.users?.response || [];
        this.totalUsers = users.length;

        const suppliers = data.suppliers?.response || [];
        this.totalSuppliers = suppliers.length;

        this.totalCategories = this.allCategories.length;

        } catch (e) {
          console.error('Dashboard data processing error:', e);
        }
        this.isLoading = false;
        this.processFilteredData();
      },
      error: (err: any) => {
        console.error('Dashboard API error:', err);
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
    this.totalRevenue = orders.reduce((sum: number, o: any) => sum + (o.totalAmount || 0), 0);
    this.resetOrderStats();
    this.computeOrderStats(orders);
    this.recentOrders = [...orders]
      .sort((a: any, b: any) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
      .slice(0, 8);
    this.computeTopProducts(orders, this.allProducts);

    this.cdr.markForCheck();
    setTimeout(() => {
      this.buildRevenueChart(orders);
      this.buildOrderDoughnut();
      this.buildDeliveryDoughnut();
      this.buildCategoryChart(orders, this.allProducts, this.allCategories);
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

  private resetOrderStats(): void {
    this.paidOrders = 0;
    this.codOrders = 0;
    this.pendingPaymentOrders = 0;
    this.pendingDeliveryOrders = 0;
    this.processingOrders = 0;
    this.shippedOrders = 0;
    this.deliveredOrders = 0;
    this.cancelledOrders = 0;
  }

  computeOrderStats(orders: any[]): void {
    for (const order of orders) {
      // Payment status
      const paymentStatus = (order.status || '').toLowerCase();
      if (paymentStatus === 'paid') this.paidOrders++;
      else if (paymentStatus === 'cod') this.codOrders++;
      else if (paymentStatus === 'pending') this.pendingPaymentOrders++;

      // Delivery status
      const deliveryStatus = (order.deliveryStatus || '').toLowerCase();
      if (deliveryStatus === 'pending') this.pendingDeliveryOrders++;
      else if (deliveryStatus === 'processing') this.processingOrders++;
      else if (deliveryStatus === 'shipped') this.shippedOrders++;
      else if (deliveryStatus === 'delivered') this.deliveredOrders++;
      else if (deliveryStatus === 'cancelled') this.cancelledOrders++;
    }
  }

  computeTopProducts(orders: any[], products: any[]): void {
    const productCount: Record<number, number> = {};
    const productRevenue: Record<number, number> = {};
    for (const order of orders) {
      // Handle both single object and array for products
      let items: any[];
      if (Array.isArray(order.products)) {
        items = order.products;
      } else {
        items = order.products ? [order.products] : [];
      }
      for (const d of items) {
        productCount[d.productId] = (productCount[d.productId] || 0) + (d.quantity || 1);
        productRevenue[d.productId] = (productRevenue[d.productId] || 0) + (d.totalPrice || d.quantity * (d.unitPrice || 0));
      }
    }
    const productMap = new Map(products.map((p: any) => [p.id, p]));
    const imageBase = environment.baseUrl.replace('/api/', '/');
    this.topProducts = Object.entries(productCount)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([id, qty]) => {
        const prod = productMap.get(+id);
        const firstImage = prod?.imageUrls?.length ? imageBase + (Array.isArray(prod.imageUrls) ? prod.imageUrls[0] : prod.imageUrls) : null;
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
      const orderDate = new Date(order.orderDate);
      const key = this.toLocalDateKey(orderDate);
      if (dayMap[key]) {
        dayMap[key].revenue += order.totalAmount || 0;
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

  buildOrderDoughnut(): void {
    this.orderDoughnutData = {
      ...this.orderDoughnutData,
      datasets: [{
        ...this.orderDoughnutData.datasets[0],
        data: [this.paidOrders, this.codOrders, this.pendingPaymentOrders]
      }]
    };
  }

  buildDeliveryDoughnut(): void {
    this.deliveryDoughnutData = {
      ...this.deliveryDoughnutData,
      datasets: [{
        ...this.deliveryDoughnutData.datasets[0],
        data: [this.pendingDeliveryOrders, this.processingOrders, this.shippedOrders, this.deliveredOrders, this.cancelledOrders]
      }]
    };
  }

  buildCategoryChart(orders: any[], products: any[], categories: any[]): void {
    // Map productId → categoryId (via product's subCategory or category)
    const productMap = new Map(products.map((p: any) => [p.id, p]));
    const categoryMap = new Map(categories.map((c: any) => [c.id, c.name]));
    const catRevenue: Record<number, number> = {};

    for (const order of orders) {
      let items: any[];
      if (Array.isArray(order.products)) {
        items = order.products;
      } else {
        items = order.products ? [order.products] : [];
      }
      for (const d of items) {
        const prod = productMap.get(d.productId);
        const catId = prod?.categoryId || 0;
        if (catId) {
          catRevenue[catId] = (catRevenue[catId] || 0) + (d.totalPrice || d.quantity * (d.unitPrice || 0));
        }
      }
    }
    const sorted = Object.entries(catRevenue).sort((a, b) => b[1] - a[1]).slice(0, 6);
    this.categorySales = sorted.map(([id, rev]) => ({
      name: categoryMap.get(+id) || `Category #${id}`,
      revenue: rev
    }));
    this.categoryChartData = {
      labels: this.categorySales.map(c => c.name),
      datasets: [{
        ...this.categoryChartData.datasets[0],
        data: this.categorySales.map(c => c.revenue)
      }]
    };
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'paid': return 'badge-paid';
      case 'cod': return 'badge-cod';
      case 'pending': return 'badge-pending';
      case 'cancelled': return 'badge-cancelled';
      case 'delivered': return 'badge-delivered';
      case 'shipped': return 'badge-shipped';
      case 'processing': return 'badge-processing';
      default: return 'badge-default';
    }
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

  getOrderPercentage(count: number): number {
    return this.totalOrders > 0 ? Math.round((count / this.totalOrders) * 100) : 0;
  }
}
