using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<PaginatedResponseDTO<InsuranceProduct>> ListProducts( PaginationQueryDTO query);

        Task<InsuranceProduct> CreateProduct(InsuranceProduct product);

        Task<InsuranceProduct?> DeactivateProduct(int productId);

        Task<InsuranceProduct?> ActivateProduct(int productId);

        Task<InsuranceProduct?> UpdateProduct( int productId, InsuranceProduct product);

        Task<InsuranceProduct?> GetProductById(int productId);

        Task<InsuranceProduct?> GetProductByProductName( string productName);
    }
}
