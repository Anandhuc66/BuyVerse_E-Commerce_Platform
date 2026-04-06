using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepo _repo;

        public ProductController(IProductRepo repo)
        {
            _repo = repo;
        }

        // -------------------------------
        // Public: anyone can view products
        // -------------------------------
        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue)
                return Ok(await _repo.GetAllProductsPaged(pageNumber.Value, pageSize.Value));

            return Ok(await _repo.GetAllProducts());
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetProductById(id);
            return result.isError ? NotFound(result) : Ok(result);
        }

        // -------------------------------
        // Supplier: view only their products
        // Admin: can view any
        // -------------------------------
        [Authorize(Roles = "Admin,Supplier")]
        [HttpGet("ByUser/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            // Supplier must only view their own data
            if (User.IsInRole("Supplier"))
            {
                var currentUser = User.FindFirst("sub")?.Value;
                if (currentUser != userId)
                    return NotFound();
            }

            var result = await _repo.GetProductsByUserId(userId);
            return result.isError ? NotFound(result) : Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpGet("BySupplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplier(int supplierId)
        {
            // SECURITY: Supplier must only view their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _repo.GetSupplierIdByUserId(userId);
                if (loggedSupplierId != supplierId)
                    return NotFound();
            }

            var result = await _repo.GetProductsBySupplierId(supplierId);
            return result.isError ? NotFound(result) : Ok(result);
        }

        // -------------------------------
        // Create product with images
        // -------------------------------
        [Authorize(Roles = "Admin,Supplier")]
        [HttpPost("add-with-images")]
        public async Task<IActionResult> CreateWithImages(
            [FromForm] ProductWithImagesCreateDto model,
            [FromServices] IWebHostEnvironment env)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Supplier must create ONLY their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;

                if (model.SupplierId <= 0)
                    return BadRequest("SupplierId is required.");

                // validate supplierId belongs to logged-in supplier
                var loggedSupplierId = await _repo.GetSupplierIdByUserId(userId);

                if (loggedSupplierId != model.SupplierId)
                    return NotFound();
            }

            var result = await _repo.AddProductWithImages(model, env);
            return result.isError ? BadRequest(result) : Ok(result);
        }

        // -------------------------------
        // Create product (JSON version)
        // -------------------------------
        [Authorize(Roles = "Admin,Supplier")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Supplier must create only their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _repo.GetSupplierIdByUserId(userId);

                if (loggedSupplierId != model.SupplierId)
                    return NotFound();
            }

            var result = await _repo.AddProduct(model);
            return result.isError ? BadRequest(result) : Ok(result);
        }

        // -------------------------------
        // Update product
        // -------------------------------
        [Authorize(Roles = "Admin,Supplier")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProductUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Supplier must update only their own products
            if (User.IsInRole("Supplier"))
            {
                var product = await _repo.GetProductById(model.Id);
                if (product.isError || product.Response == null)
                    return NotFound(product);

                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _repo.GetSupplierIdByUserId(userId);

                if (product.Response.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.UpdateProduct(model);
            return result.isError ? BadRequest(result) : Ok(result);
        }

        // -------------------------------
        // Delete product
        // -------------------------------
        [Authorize(Roles = "Admin,Supplier")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Supplier must delete only their own products
            if (User.IsInRole("Supplier"))
            {
                var product = await _repo.GetProductById(id);
                if (product.isError || product.Response == null)
                    return NotFound(product);

                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _repo.GetSupplierIdByUserId(userId);

                if (product.Response.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.DeleteProduct(id);
            return result.isError ? BadRequest(result) : Ok(result);
        }
        // --------------------------------------------
        // Get Products by SubCategoryId (Public Access)
        // --------------------------------------------
        [AllowAnonymous]
        [HttpGet("SubCategory/{subCategoryId}")]
        public async Task<IActionResult> GetBySubCategory(int subCategoryId)
        {
            var result = await _repo.GetProductsBySubCategoryId(subCategoryId);

            if (result.isError)
                return NotFound(result);

            return Ok(result);
        }
        // --------------------------------------------
        // 🔍 Search Products (Public Access)
        // --------------------------------------------
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Keyword is required.");

            var result = await _repo.SearchProducts(keyword);

            if (result.isError)
                return NotFound(result);

            return Ok(result);
        }


    }
}
