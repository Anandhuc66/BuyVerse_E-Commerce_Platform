# Buyverse — Full-Stack E-Commerce Application

A complete e-commerce platform built with **Angular 20** (frontend) and **ASP.NET Core 8 Web API** (backend), featuring role-based access control, Razorpay payment integration, and dedicated dashboards for Admins, Suppliers, and Users.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Payment Integration](#payment-integration)
- [Screenshots](#screenshots)
- [License](#license)

---

## Features

### Customer
- Browse products by category/subcategory
- Search and filter products
- Product detail pages with image gallery
- Product ratings & reviews
- Shopping cart management
- Checkout with address selection
- Online payment via Razorpay or Cash on Delivery
- Order history & tracking
- User profile management

### Admin
- Dashboard with analytics charts
- Manage products, categories & subcategories
- Manage suppliers and users
- Role management (CRUD)
- View and manage all orders
- Update delivery status

### Supplier
- Dedicated registration & dashboard
- Add/manage own products with multiple images
- View orders containing their products
- Profile settings

### General
- JWT-based authentication
- Role-based authorization (Admin, Supplier, User)
- Server-side pagination
- Response caching on public endpoints
- Global error handling middleware
- PDF invoice generation
- File upload for product images

---

## Tech Stack

### Frontend

| Technology | Version | Purpose |
|---|---|---|
| Angular | 20 | SPA framework |
| TypeScript | 5.8 | Type-safe JavaScript |
| Bootstrap | 5.3 | CSS framework |
| Bootstrap Icons | 1.13 | Icon set |
| Tailwind CSS | 3.4 | Utility CSS |
| Angular Material | 20 | UI components (dialogs) |
| AG Grid | 34 | Data tables |
| Chart.js + ng2-charts | 4.5 / 8.0 | Dashboard charts |
| ngx-toastr | 19 | Toast notifications |
| RxJS | 7.8 | Reactive programming |

### Backend

| Technology | Version | Purpose |
|---|---|---|
| .NET | 8 | Runtime |
| ASP.NET Core Web API | 8 | REST API framework |
| Entity Framework Core | 8 | ORM |
| SQL Server | — | Database |
| ASP.NET Core Identity | 8 | User & role management |
| JWT Bearer | 8 | Token authentication |
| Razorpay SDK | 3.3 | Payment gateway |
| QuestPDF | 2025.7 | PDF invoice generation |
| Swashbuckle | 6.6 | Swagger / OpenAPI docs |

---

## Architecture

```
┌───────────────────────┐          ┌───────────────────────────────┐
│   Angular Frontend    │  HTTP    │     ASP.NET Core Web API      │
│                       │ ◄──────► │                               │
│  • User Module        │  JWT     │  Controllers (14)             │
│  • Admin Module       │          │  Repository Layer (15)        │
│  • Supplier Module    │          │  Entity Models (14)           │
│  • Core (Auth/Guards) │          │  Identity + JWT Auth          │
└───────────────────────┘          │  EF Core + SQL Server         │
                                   │  Razorpay Integration         │
                                   │  QuestPDF Invoices            │
                                   └───────────────────────────────┘
```

### Solution Projects

| Project | Purpose |
|---|---|
| `Ecommerce_Api` | API controllers, middleware, startup configuration |
| `Ecommerce_Entity` | EF Core models, DTOs, DbContext, migrations |
| `Ecommerce_Service` | Repository interfaces & implementations |
| `Ecommerce_Common` | Shared types: `Result<T>`, `PagedResult<T>`, `Errors` |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18+)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (Express or higher)

### Backend Setup

```bash
# Navigate to the API project
cd Ecommerce_Api/Ecommerce_Api

# Restore packages
dotnet restore

# Update the connection string in appsettings.Development.json if needed
# Default: Server=.\SQLEXPRESS;Database=BuyverseE-Commerce;Trusted_Connection=True

# Apply database migrations
dotnet ef database update

# Run the API (starts at http://localhost:5189)
dotnet run
```

> Swagger UI is available at `http://localhost:5189/swagger` in development mode.

On first run, the application automatically seeds:
- Three roles: **Admin**, **Supplier**, **User**
- A default admin account

### Frontend Setup

```bash
# Navigate to the Angular project
cd E-commerce-App-UI/ECommerce-App

# Install dependencies
npm install

# Start the dev server (runs at http://localhost:1200)
ng serve
```

> The frontend expects the API at `http://localhost:5189/api/`. Update `src/environments/environment.ts` if your API runs on a different port.

---

## Project Structure

### Frontend (`E-commerce-App-UI/ECommerce-App/src/app/`)

```
app/
├── core/                    # Shared across all modules
│   ├── components/          # Header, Footer, CatNavbar
│   ├── guards/              # authGuard, roleGuard, roleRedirectGuard
│   ├── interceptors/        # TokenInterceptor (attaches JWT)
│   └── services/            # AuthService
├── components/              # Standalone: Login, Register, ConfirmDialog
├── models/                  # TypeScript interfaces
├── modules/
│   ├── user/                # Customer-facing pages
│   │   ├── components/      # Homepage, ProductList, ProductDetails,
│   │   │                    # Cart, Checkout, Orders, Profile,
│   │   │                    # PaymentSuccess, PaymentFailure
│   │   └── services/        # User API service
│   ├── admin/               # Admin dashboard
│   │   ├── components/      # Dashboard, Products, Categories,
│   │   │                    # Suppliers, Users, Roles, Orders, Settings
│   │   └── services/        # Admin API service
│   └── supplier/            # Supplier dashboard
│       ├── components/      # Dashboard, Products, Orders, Settings,
│       │                    # SupplierRegister
│       └── services/        # Supplier API service
└── layouts/                 # MainLayout, AuthLayout (per module)
```

### Backend (`Ecommerce_Api/`)

```
Ecommerce_Api/
├── Controllers/             # 14 API controllers
├── Middleware/               # ExceptionHandlingMiddleware
├── wwwroot/uploads/         # Product image storage
Ecommerce_Entity/
├── Models/                  # 14 entity models
├── DTO/                     # 16 DTO files
├── Migrations/              # EF Core migrations
Ecommerce_Service/
├── Repository/              # 15 interfaces + 15 implementations
│                            # + InvoiceService (PDF)
Ecommerce_Common/
├── Result.cs                # Generic API response wrapper
├── PagedResult.cs           # Pagination wrapper
├── Error.cs                 # Error model
```

---

## API Endpoints

| Controller | Route | Description |
|---|---|---|
| Auth | `POST /api/Auth/register` | User registration |
| Auth | `POST /api/Auth/register-supplier` | Supplier registration |
| Auth | `POST /api/Auth/login` | Login (returns JWT) |
| Auth | `GET /api/Auth/users` | Get all users (Admin) |
| Product | `GET /api/Product` | List all products |
| Product | `POST /api/Product` | Create product with images |
| Product | `GET /api/Product/search` | Search products |
| Product | `GET /api/Product/subcategory/{id}` | Filter by subcategory |
| Category | `GET /api/Category` | List categories (cached) |
| SubCategory | `GET /api/SubCategory` | List subcategories (cached) |
| Cart | `GET/POST/PUT/DELETE /api/Cart` | Cart operations |
| Order | `POST /api/Order` | Create order |
| Order | `GET /api/Order` | Get orders (role-filtered) |
| Order | `GET /api/Order/invoice/{id}` | Download PDF invoice |
| Payment | `POST /api/Payment` | Record payment |
| Razorpay | `POST /api/Razorpay/create-order` | Create Razorpay order |
| Razorpay | `POST /api/Razorpay/verify` | Verify payment signature |
| ProductImage | `POST /api/ProductImage` | Upload product images |
| ProductReview | `GET/POST/PUT/DELETE /api/ProductReview` | Product reviews |
| UserAddress | `GET/POST/PUT/DELETE /api/UserAddress` | User addresses |
| Supplier | `GET/POST/PUT/DELETE /api/Supplier` | Supplier management |
| Role | `GET/POST/PUT/DELETE /api/Role` | Role management (Admin) |

---

## Authentication & Authorization

- **JWT Bearer tokens** with configurable expiration (120 min dev / 60 min prod)
- **Three roles**: Admin, Supplier, User — seeded on startup
- **Token interceptor** on the frontend automatically attaches the JWT to all API requests
- **Auth guard** protects authenticated routes; **role guard** enforces role-based access
- **Server-side ownership checks** ensure users can only access their own data

---

## Payment Integration

Buyverse supports two payment methods:

1. **Razorpay** — Online payment via Razorpay checkout widget
   - Backend creates a Razorpay order with server-validated amount
   - Frontend opens Razorpay checkout
   - Backend verifies payment signature before confirming
   - Duplicate payment prevention built-in

2. **Cash on Delivery (COD)** — Order placed without online payment

---

## Screenshots

*Add screenshots of your application here.*

---

## License

This project is for educational/portfolio purposes.
