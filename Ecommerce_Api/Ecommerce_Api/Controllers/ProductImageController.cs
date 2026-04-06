using Ecommerce_Entity.Data;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageRepo _repo;
        private readonly IProductRepo _productRepo;
        private readonly IWebHostEnvironment _env;
        public ProductImageController(IProductImageRepo repo, IProductRepo productRepo, IWebHostEnvironment env)
        {
            _env = env;
            _repo = repo;
            _productRepo = productRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAll());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductImageCreateDto model)
        {
            // SECURITY: Supplier can only add images to their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _productRepo.GetSupplierIdByUserId(userId);
                var product = await _productRepo.GetProductById(model.ProductId);
                if (product.isError || product.Response?.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.Add(model, _env);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] ProductImageUploadDto model)
        {
            // SECURITY: Supplier can only upload images to their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _productRepo.GetSupplierIdByUserId(userId);
                var product = await _productRepo.GetProductById(model.ProductId);
                if (product.isError || product.Response?.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.AddMultipleFiles(model, _env);

            if (result.Errors.Any())
                return BadRequest(result.Errors);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProductImageUpdateDto model)
        {
            // SECURITY: Supplier can only update images of their own products
            if (User.IsInRole("Supplier"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _productRepo.GetSupplierIdByUserId(userId);
                var product = await _productRepo.GetProductById(model.ProductId);
                if (product.isError || product.Response?.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.Update(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Supplier")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // SECURITY: Supplier can only delete images of their own products
            if (User.IsInRole("Supplier"))
            {
                var image = await _repo.GetById(id);
                if (image.isError) return NotFound(image);

                var userId = User.FindFirst("sub")?.Value;
                var loggedSupplierId = await _productRepo.GetSupplierIdByUserId(userId);
                var product = await _productRepo.GetProductById(image.Response.ProductId);
                if (product.isError || product.Response?.SupplierId != loggedSupplierId)
                    return NotFound();
            }

            var result = await _repo.Delete(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
