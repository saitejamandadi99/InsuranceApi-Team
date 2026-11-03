using AutoMapper;
using Insurance_Backend.Custom;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;

namespace Insurance_Backend.Services.Classes
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepo,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductResponseDTO?> ActivateProduct(int productId)
        {
            var result = await _productRepo.ActivateProduct(productId);

            if (result == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "Product {ProductId} - {ProductName} activated",
                result.ProductId,
                result.ProductName);

            return _mapper.Map<ProductResponseDTO>(result);
        }

        public async Task<ProductResponseDTO> CreateProduct(
            ProductRequestDTO request)
        {
            var existingProduct =
                await _productRepo.GetProductByProductName(request.ProductName);

            if (existingProduct != null)
            {
                throw new BusinessException(
                    "product with this name already exists",
                    StatusCodes.Status409Conflict);
            }

            var product = _mapper.Map<InsuranceProduct>(request);

            // Preserve the existing create behavior.
            product.ActiveStatus = ActiveStatus.Active;

            var result = await _productRepo.CreateProduct(product);

            _logger.LogInformation(
                "Product {ProductId} - {ProductName} created",
                result.ProductId,
                result.ProductName);

            return _mapper.Map<ProductResponseDTO>(result);
        }

        public async Task<ProductResponseDTO?> DeactivateProduct(int productId)
        {
            var result = await _productRepo.DeactivateProduct(productId);

            if (result == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "Product {ProductId} - {ProductName} deactivated",
                result.ProductId,
                result.ProductName);

            return _mapper.Map<ProductResponseDTO>(result);
        }

        public async Task<ProductResponseDTO?> GetProductById(int productId)
        {
            var product = await _productRepo.GetProductById(productId);

            if (product == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<ProductResponseDTO?> GetProductByProductName(
            string productName)
        {
            var product =
                await _productRepo.GetProductByProductName(productName);

            if (product == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<PaginatedResponseDTO<ProductResponseDTO>> ListProducts(
            PaginationQueryDTO query)
        {
            var result = await _productRepo.ListProducts(query);

            return new PaginatedResponseDTO<ProductResponseDTO>
            {
                Records = _mapper.Map<IEnumerable<ProductResponseDTO>>(
                    result.Records),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        public async Task<ProductResponseDTO?> UpdateProduct(
            int productId,
            ProductRequestDTO request)
        {
            var existingProduct =
                await _productRepo.GetProductById(productId);

            if (existingProduct == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            var duplicateProduct =
                await _productRepo.GetProductByProductName(request.ProductName);

            if (duplicateProduct != null &&
                duplicateProduct.ProductId != productId)
            {
                throw new BusinessException(
                    "Product with this name already exists.",
                    StatusCodes.Status409Conflict);
            }

            // Map the request onto the existing entity so its ID and
            // other non-request properties remain intact.
            _mapper.Map(request, existingProduct);

            var result = await _productRepo.UpdateProduct(
                productId,
                existingProduct);

            if (result == null)
            {
                throw new BusinessException(
                    "Product not found.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "Product {ProductId} - {ProductName} updated",
                result.ProductId,
                result.ProductName);

            return _mapper.Map<ProductResponseDTO>(result);
        }
    }
}
