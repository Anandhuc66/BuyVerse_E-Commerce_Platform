using Ecommerce_Entity.Data;
using Ecommerce_Entity.Models;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
// ============================================================
//  SWAGGER + JWT INTEGRATION
// ============================================================
builder.Services.AddSwaggerGen(c =>
{
    //  Add Bearer Token Authorization to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// ============================================================
//  JWT AUTHENTICATION CONFIGURATION
// ============================================================
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Warn if default JWT key is used in non-Development
if (!builder.Environment.IsDevelopment() &&
    jwtSettings["Key"] == "ThisIsASuperSecretKeyForJWT12345!")
{
    throw new InvalidOperationException(
        "Production must not use the default JWT key. " +
        "Set Jwt__Key environment variable or update appsettings.Production.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Enforce HTTPS in production
    options.SaveToken = true;
    options.MapInboundClaims = false; // Keep short JWT claim names (sub, role, etc.)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // ensure token expiry is checked
        ValidateIssuerSigningKey = true, // ensure token signature validity
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"])
        ),
        RoleClaimType = "role",
        NameClaimType = "name"
    };
});

// ============================================================
//  DATABASE CONTEXT
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DataContextConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120)
    )
);

// ============================================================
//  IDENTITY CONFIGURATION
// ============================================================
builder.Services.AddIdentity<ApplicationUser, AppRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
//  PREVENT IDENTITY COOKIE 302 REDIRECTS FOR API CALLS
// ============================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ============================================================
//  DEPENDENCY INJECTION (REPOSITORIES)
// ============================================================
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<ISupplierRepo, SupplierRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderDetailRepo, OrderDetailRepo>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<ICartRepo, CartRepo>();
builder.Services.AddScoped<IRoleRepo, RoleRepo>();
builder.Services.AddScoped<IProductImageRepo, ProductImageRepo>();
builder.Services.AddScoped<IUserAddressRepo, UserAddressRepo>();
builder.Services.AddScoped<ISubCategoryRepo, SubCategoryRepo>();
builder.Services.AddScoped<IProductReviewRepo, ProductReviewRepo>();
builder.Services.AddScoped<IRazorpayService, RazorpayServiceRepo>();
builder.Services.AddScoped<InvoiceService>();





// ADD THIS BEFORE THE APP BUILDS
QuestPDF.Settings.License = LicenseType.Community;

// ============================================================
//  RESPONSE CACHING
// ============================================================
builder.Services.AddResponseCaching();


// ============================================================
//  CONTROLLERS & JSON SETTINGS
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ============================================================
//  CORS POLICY
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:1200" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();

// ============================================================
// STATIC FILES CONFIG
// ============================================================
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// ============================================================
//  MIDDLEWARE PIPELINE
// ============================================================
// ============================================================
//  GLOBAL EXCEPTION MIDDLEWARE
// ============================================================
app.UseMiddleware<Ecommerce_Api.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAngularApp");

app.UseResponseCaching();

app.UseAuthentication();  //  must come before Authorization
app.UseAuthorization();

// ============================================================
// SEED ROLES & ADMIN ACCOUNT
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdmin(services);
}

// ============================================================
//  MAP CONTROLLERS
// ============================================================
app.MapControllers();

app.Run();

// ============================================================
//  HELPER METHOD: Seed Roles & Admin
// ============================================================
async Task SeedRolesAndAdmin(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Supplier", "User" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new AppRole
            {
                Name = roleName,
                DisplayName = roleName,
                Description = $"Default {roleName} role"
            });
        }
    }

    // Create Default Admin User (credentials from env vars or appsettings, with fallback for dev only)
    string adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@ecommerce.com";
    string adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Admin@123";

    if (!app.Environment.IsDevelopment() && adminPassword == "Admin@123")
    {
        Console.WriteLine("WARNING: Using default admin password in non-development environment. Set AdminSeed__Password env variable.");
    }

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Main Admin",
            Gender = "Male",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
