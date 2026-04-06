using Ecommerce_Common;
using Ecommerce_Entity.DTO;
using Ecommerce_Service.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductReviewController : ControllerBase
    {
        private readonly IProductReviewRepo _repo;

        public ProductReviewController(IProductReviewRepo repo)
        {
            _repo = repo;
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _repo.GetByProductId(productId);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _repo.GetById(id);
            if (result.isError) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductReviewCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Force userId from token
            model.UserId = User.FindFirst("sub")?.Value;

            var result = await _repo.Add(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProductReviewUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ownership: only review author or Admin can update
            if (User.IsInRole("User"))
            {
                var existing = await _repo.GetById(model.Id);
                if (existing.isError || existing.Response?.UserId != User.FindFirst("sub")?.Value)
                    return NotFound(new { message = "Review not found" });
            }

            var result = await _repo.Update(model);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ownership: only review author or Admin can delete
            if (User.IsInRole("User"))
            {
                var existing = await _repo.GetById(id);
                if (existing.isError || existing.Response?.UserId != User.FindFirst("sub")?.Value)
                    return NotFound(new { message = "Review not found" });
            }

            var result = await _repo.Delete(id);
            if (result.isError) return BadRequest(result);
            return Ok(result);
        }
    }
}
