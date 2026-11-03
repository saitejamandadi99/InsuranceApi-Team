using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedResponseDTO<ProductResponseDTO>> ListProducts( PaginationQueryDTO query);

        Task<ProductResponseDTO?> GetProductById(int productId);

        Task<ProductResponseDTO?> GetProductByProductName(string productName);

        Task<ProductResponseDTO> CreateProduct(ProductRequestDTO request);

        Task<ProductResponseDTO?> UpdateProduct( int productId, ProductRequestDTO request);

        Task<ProductResponseDTO?> ActivateProduct(int productId);

        Task<ProductResponseDTO?> DeactivateProduct(int productId);
    }
}
