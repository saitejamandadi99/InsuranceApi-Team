using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IClaimDocumentRepository
    {
        Task<ClaimDocument> AddClaimDocument(ClaimDocument document);

        Task<IEnumerable<ClaimDocument>> GetDocumentsByClaimId(int claimId);

        Task<ClaimDocument?> GetDocumentById(int documentId);

        Task<IEnumerable<ClaimDocument>> GetDocumentsByCustomerId(int customerId);

        Task<PaginatedResponseDTO<ClaimDocument>> ListDocuments(
            PaginationQueryDTO query);

        Task DeleteDocument(ClaimDocument document);
    }
}
