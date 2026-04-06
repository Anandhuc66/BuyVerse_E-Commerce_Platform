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
    public class ProductReviewRepo : IProductReviewRepo
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // Get reviews by product
        // ========================
        public async Task<Result<List<ProductReviewDto>>> GetByProductId(int productId)
        {
            var result = new Result<List<ProductReviewDto>>();

            var reviews = await _context.ProductReviewsSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            if (!reviews.Any())
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "No reviews found for this product." });
                return result;
            }

            result.Response = reviews;
            return result;
        }

        // ========================
        // Get review by ID
        // ========================
        public async Task<Result<ProductReviewDto>> GetById(int id)
        {
            var result = new Result<ProductReviewDto>();

            var review = await _context.ProductReviewsSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.Id == id)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Review not found." });
                return result;
            }

            result.Response = review;
            return result;
        }

        // ========================
        // Add review
        // ========================
        public async Task<Result<ProductReview>> Add(ProductReviewCreateDto model)
        {
            var result = new Result<ProductReview>();

            // Validate user
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "User not found." });
                return result;
            }

            // Validate product
            var product = await _context.ProductsSet.FindAsync(model.ProductId);
            if (product == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Product not found." });
                return result;
            }

            // Prevent duplicate review from same user
            var existingReview = await _context.ProductReviewsSet
                .AnyAsync(r => r.UserId == model.UserId && r.ProductId == model.ProductId);

            if (existingReview)
            {
                result.Errors.Add(new Errors { ErrorCode = 409, ErrorMessage = "User already reviewed this product." });
                return result;
            }

            var review = new ProductReview
            {
                ProductId = model.ProductId,
                UserId = model.UserId,
                Rating = model.Rating,
                Title = model.Title,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductReviewsSet.AddAsync(review);
            await _context.SaveChangesAsync();

            result.Response = review;
            result.Message = "Review added successfully.";

            return result;
        }

        // ========================
        // Update review
        // ========================
        public async Task<Result<ProductReview>> Update(ProductReviewUpdateDto model)
        {
            var result = new Result<ProductReview>();

            var review = await _context.ProductReviewsSet.FindAsync(model.Id);
            if (review == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Review not found." });
                return result;
            }

            review.Rating = model.Rating;
            review.Title = model.Title;
            review.Comment = model.Comment;
            // Do NOT overwrite CreatedAt — it should remain the original creation time

            _context.ProductReviewsSet.Update(review);
            await _context.SaveChangesAsync();

            result.Response = review;
            result.Message = "Review updated successfully.";
            return result;
        }

        // ========================
        // Delete review
        // ========================
        public async Task<Result<bool>> Delete(int id)
        {
            var result = new Result<bool>();

            var review = await _context.ProductReviewsSet.FindAsync(id);
            if (review == null)
            {
                result.Errors.Add(new Errors { ErrorCode = 404, ErrorMessage = "Review not found." });
                return result;
            }

            _context.ProductReviewsSet.Remove(review);
            await _context.SaveChangesAsync();

            result.Response = true;
            result.Message = "Review deleted successfully.";
            return result;
        }
    }
}
