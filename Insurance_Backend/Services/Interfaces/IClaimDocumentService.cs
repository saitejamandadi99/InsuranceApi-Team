using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using System.Security.Claims;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IClaimDocumentService
    {
        Task<PaginatedResponseDTO<ClaimDocumentResponseDTO>> ListDocuments(PaginationQueryDTO query);
        Task<ClaimDocumentResponseDTO?> GetDocumentById(int documentId);
        Task<IEnumerable<ClaimDocumentResponseDTO>> GetDocumentsByClaimId(int claimId);
        Task<IEnumerable<ClaimDocumentResponseDTO>> GetDocumentsByCustomerId(int customerId);

        Task<IEnumerable<ClaimDocumentResponseDTO>> AddDocument(ClaimDocumentRequestDTO request, ClaimsPrincipal user);

        Task DeleteDocument(int documentId, ClaimsPrincipal user);


        Task<IEnumerable<ClaimDocumentResponseDTO>> GetDocumentsByUserId(int userId);


    }
}
