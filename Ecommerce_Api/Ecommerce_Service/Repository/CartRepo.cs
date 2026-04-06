using Ecommerce_Common;
using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Entity.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Service.Repository
{
    public class CartRepo : ICartRepo
    {
        private readonly ApplicationDbContext _context;
        public CartRepo(ApplicationDbContext context) => _context = context;

        // GET ALL CART ITEMS (ADMIN)
        // ======================================================
        public async Task<Result<List<Cart>>> GetAll()
        {
            var result = new Result<List<Cart>>();

            var carts = await _context.CartsSet
                .Include(c => c.User)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
                .Include(c => c.Product.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .ToListAsync();

            if (carts.Any())
                result.Response = carts;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No carts found" });

            return result;
        }
        // GET CART BY USER
        // ======================================================
        public async Task<Result<List<CartItemWithUserDto>>> GetByUserId(string userId)
        {
            var result = new Result<List<CartItemWithUserDto>>();

            var carts = await _context.CartsSet
                .Where(c => c.UserId == userId)
                .Include(c => c.User)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
                .Include(c => c.Product.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(c => c.Product.Supplier)
                .Select(c => new CartItemWithUserDto
                {
                    // Cart
                    CartId = c.CartId,
                    Quantity = c.Quantity,
                    CartCreatedAt = c.CreatedAt,

                    // User
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    Gender = c.User.Gender,
                    UserCreatedAt = c.User.CreatedAt,                  

                    // Product
                    ProductId = c.Product.Id,
                    ProductName = c.Product.Name,
                    ProductPrice = c.Product.Price,

                    // Category
                    CategoryId = c.Product.SubCategory.Category.Id,
                    CategoryName = c.Product.SubCategory.Category.Name,

                    // Supplier
                    ProductSupplierId = c.Product.SupplierId,
                    ProductSupplierName = c.Product.Supplier.CompanyName,

                    // Images
                    ImageUrls = c.Product.Images.Select(i => i.ImageUrl).ToList()
                })
                .ToListAsync();

            if (carts.Any())
                result.Response = carts;
            else
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "No cart items found for this user"
                });

            return result;
        }


        // GET BY CART ID
        // ======================================================
        public async Task<Result<Cart>> GetById(int id)
        {
            var result = new Result<Cart>();

            var cart = await _context.CartsSet
                .Include(c => c.User)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CartId == id);

            if (cart != null)
                result.Response = cart;
            else
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Cart not found" });

            return result;
        }

        // ADD TO CART
        // ======================================================
        public async Task<Result<Cart>> Add(CartCreateDto model)
        {
            var result = new Result<Cart>();

            // Check if product exists
            var product = await _context.ProductsSet
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (product == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Product not found"
                });
                return result;
            }

            // Check duplicate
            var existing = await _context.CartsSet
                .FirstOrDefaultAsync(c => c.UserId == model.UserId &&
                                          c.ProductId == model.ProductId);

            if (existing != null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 409,
                    ErrorMessage = "Product already exists in your cart"
                });

                return result;
            }

            // Check stock
            if (product.StockQuantity < model.Quantity)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 400,
                    ErrorMessage = "Insufficient stock"
                });
                return result;
            }

            // Add item
            var cart = new Cart
            {
                UserId = model.UserId,
                ProductId = model.ProductId,
                Quantity = model.Quantity
            };

            await _context.CartsSet.AddAsync(cart);
            await _context.SaveChangesAsync();

            result.Response = cart;
            result.Message = "Cart added successfully";

            return result;
        }



        // UPDATE CART (ONLY QUANTITY)
        // ======================================================
        public async Task<Result<Cart>> Update(CartUpdateDto model)
        {
            var result = new Result<Cart>();

            var cart = await _context.CartsSet.FindAsync(model.CartId);
            if (cart == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Cart not found"
                });
                return result;
            }

            if (model.Quantity < 1)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 400,
                    ErrorMessage = "Quantity cannot be less than 1"
                });
                return result;
            }

            // Validate against product stock
            var product = await _context.ProductsSet.FindAsync(cart.ProductId);
            if (product == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 400,
                    ErrorMessage = "Product no longer exists"
                });
                return result;
            }
            if (model.Quantity > product.StockQuantity)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 400,
                    ErrorMessage = $"Requested quantity exceeds available stock ({product.StockQuantity})"
                });
                return result;
            }

            cart.Quantity = model.Quantity;

            _context.CartsSet.Update(cart);
            await _context.SaveChangesAsync();

            result.Response = cart;
            result.Message = "Cart updated successfully";

            return result;
        }

        // DELETE CART ITEM
        // ======================================================
        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();

            var cart = await _context.CartsSet.FindAsync(id);

            if (cart == null)
            {
                result.Errors.Add(new Errors
                {
                    ErrorCode = 404,
                    ErrorMessage = "Cart not found"
                });
                return result;
            }

            _context.CartsSet.Remove(cart);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Cart deleted successfully";

            return result;
        }

        public async Task<Result<bool>> ClearByUserId(string userId)
        {
            var result = new Result<bool>();
            var carts = await _context.CartsSet.Where(c => c.UserId == userId).ToListAsync();
            if (!carts.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No cart items found for this user" });
                return result;
            }
            _context.CartsSet.RemoveRange(carts);
            await _context.SaveChangesAsync();
            result.Response = true;
            result.Message = $"Cleared {carts.Count} cart items.";
            return result;
        }
    }
}
