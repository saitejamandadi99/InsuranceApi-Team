using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IClaimDocumentRepository
    {
        Task<ClaimDocumentResponseDTO?> AddClaimDocument(ClaimDocument document);

        Task<IEnumerable<ClaimDocumentResponseDTO?>> GetDocumentsByClaimId(int claimId);

        Task<ClaimDocumentResponseDTO?> GetDocumentById(int documentId);

        Task<IEnumerable<ClaimDocumentResponseDTO?>> GetDocumentsByCustomerId(int customerId);

        Task<PaginatedResponseDTO<ClaimDocumentResponseDTO?>> ListDocuments(PaginationQueryDTO query);

        Task DeleteDocument(ClaimDocument document);
    }
}
