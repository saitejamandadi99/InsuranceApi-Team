using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IProductService _insuService;
        public ProductController(IProductService insuService)
        {
            _insuService = insuService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ListProducts([FromQuery] PaginationQueryDTO query)
        {
            var products = await _insuService.ListProducts(query);
            return Success(products, "Retrieved products successfully");
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _insuService.GetProductById(id);
            return Success(product, "Retrieved product successfully");

        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductRequestDTO request)
        {
            var product = await _insuService.CreateProduct(request);
            return CreatedAtSuccess(product, "product created successfully", nameof(GetProductById), new { id = product.ProductId });
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductRequestDTO request)
        {
            var product = await _insuService.UpdateProduct(id, request);
            return Success(product, "Retrieved product successfully");

        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateProduct(int id)
        {
            var product = await _insuService.ActivateProduct(id);
            return Success(product, "Activated product successfully");

        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateProduct(int id)
        {
            var product = await _insuService.DeactivateProduct(id);
            return Success(product, "Deactivated products successfully");

        }

    }
}

