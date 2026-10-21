using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<PaginatedResponseDTO<ProductResponseDTO>> ListProducts(PaginationQueryDTO query);
        Task<ProductResponseDTO> CreateProduct(InsuranceProduct product);
        Task<ProductResponseDTO?> DeactivateProduct(int productId);
        Task<ProductResponseDTO?> ActivateProduct(int productId);
        Task<ProductResponseDTO?> UpdateProduct(int productId, ProductRequestDTO product);
        Task<ProductResponseDTO?> GetProductById(int productId);

        Task<ProductResponseDTO?> GetProductByProductName(string productName);
    }
}
