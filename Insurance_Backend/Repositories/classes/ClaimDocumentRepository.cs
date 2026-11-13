using Insurance_Backend.Custom;
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly AppDbContext _context;

        public ClaimDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Select only the required columns, including ClaimId.
        private IQueryable<ClaimDocument> DocumentSummaryQuery()
        {
            return _context.ClaimDocument
                .AsNoTracking()
                .Select(d => new ClaimDocument
                {
                    DocumentId = d.DocumentId,
                    ClaimId = d.ClaimId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    DocumentReference = d.DocumentReference,
                    UploadedDate = d.UploadedDate,
                    Claim = new Claim
                    {
                        ClaimId = d.Claim.ClaimId,
                        ClaimNumber = d.Claim.ClaimNumber,
                        Policy = new Policy
                        {
                            Customer = new CustomerProfile
                            {
                                UserId = d.Claim.Policy.Customer.UserId
                            }
                        }
                    }
                });
        }

        public async Task<ClaimDocument> AddClaimDocument(
            ClaimDocument document)
        {
            _context.ClaimDocument.Add(document);
            await _context.SaveChangesAsync();

            return await DocumentSummaryQuery()
                .FirstAsync(d => d.DocumentId == document.DocumentId);
        }

        public async Task<IEnumerable<ClaimDocument>> GetDocumentsByClaimId(
            int claimId)
        {
            return await DocumentSummaryQuery()
                .Where(d => d.ClaimId == claimId)
                .ToListAsync();
        }

        public async Task<ClaimDocument?> GetDocumentById(int documentId)
        {
            return await DocumentSummaryQuery()
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }

        public async Task<IEnumerable<ClaimDocument>> GetDocumentsByCustomerId(
            int customerId)
        {
            return await _context.ClaimDocument
                .AsNoTracking()
                .Where(d => d.Claim.Policy.CustomerId == customerId)
                .Select(d => new ClaimDocument
                {
                    DocumentId = d.DocumentId,
                    ClaimId = d.ClaimId,
                    DocumentName = d.DocumentName,
                    DocumentType = d.DocumentType,
                    DocumentReference = d.DocumentReference,
                    UploadedDate = d.UploadedDate
                })
                .ToListAsync();
        }

        public async Task<PaginatedResponseDTO<ClaimDocument>> ListDocuments(
     PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            string? search = query.Search;

            string sortBy = query.SortBy ?? "DocumentId";
            string sortDirection = query.SortDirection ?? "ASC";

            // Execute stored procedure
            var results = await _context.Database
                .SqlQuery<ClaimDocumentListResult>($"""
            EXEC sp_ListClaimDocuments
                @PageNumber = {pageNumber},
                @PageSize = {pageSize},
                @Search = {search},
                @SortBy = {sortBy},
                @SortDirection = {sortDirection}
            """)
                .ToListAsync();

            // Get total records
            int totalRecords = results.FirstOrDefault()?.TotalRecords ?? 0;

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            // Map results to ClaimDocument
            var documents = results.Select(d => new ClaimDocument
            {
                DocumentId = d.DocumentId,
                DocumentName = d.DocumentName,
                DocumentType = d.DocumentType,
                DocumentReference = d.DocumentReference,
                UploadedDate = d.UploadedDate,

                ClaimId = 0,

                Claim = new Claim
                {
                    ClaimNumber = d.ClaimNumber
                }

            }).ToList();

            return new PaginatedResponseDTO<ClaimDocument>
            {
                Records = documents,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }

        public async Task DeleteDocument(ClaimDocument document)
        {
            _context.ClaimDocument.Remove(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ClaimDocument>> GetDocumentsByUserId(
    int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            return await DocumentSummaryQuery()
                .Where(d => d.Claim.Policy.Customer.UserId == userId)
                .OrderByDescending(d => d.UploadedDate)
                .ThenByDescending(d => d.DocumentId)
                .ToListAsync();
        }
    }
}