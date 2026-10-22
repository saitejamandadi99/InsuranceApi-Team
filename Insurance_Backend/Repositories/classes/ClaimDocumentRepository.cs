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
                    UploadedDate = d.UploadedDate
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

            IQueryable<ClaimDocument> documents =
                _context.ClaimDocument.AsNoTracking();

            // Searching
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim().ToLower();

                documents = documents.Where(d =>
                    d.DocumentName.ToLower().Contains(search) ||
                    d.DocumentType.ToLower().Contains(search) ||
                    d.Claim.ClaimNumber.ToLower().Contains(search) ||
                    d.Claim.Policy.Customer.User.FullName
                        .ToLower().Contains(search));
            }

            // Sorting
            documents = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("documentname", "desc") =>
                    documents.OrderByDescending(d => d.DocumentName)
                             .ThenBy(d => d.DocumentId),

                ("documentname", _) =>
                    documents.OrderBy(d => d.DocumentName)
                             .ThenBy(d => d.DocumentId),

                ("documenttype", "desc") =>
                    documents.OrderByDescending(d => d.DocumentType)
                             .ThenBy(d => d.DocumentId),

                ("documenttype", _) =>
                    documents.OrderBy(d => d.DocumentType)
                             .ThenBy(d => d.DocumentId),

                ("uploadeddate", "desc") =>
                    documents.OrderByDescending(d => d.UploadedDate)
                             .ThenBy(d => d.DocumentId),

                ("uploadeddate", _) =>
                    documents.OrderBy(d => d.UploadedDate)
                             .ThenBy(d => d.DocumentId),

                ("claimnumber", "desc") =>
                    documents.OrderByDescending(d => d.Claim.ClaimNumber)
                             .ThenBy(d => d.DocumentId),

                ("claimnumber", _) =>
                    documents.OrderBy(d => d.Claim.ClaimNumber)
                             .ThenBy(d => d.DocumentId),

                _ => documents.OrderBy(d => d.DocumentId)
            };

            int totalRecords = await documents.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedDocuments = await documents
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            return new PaginatedResponseDTO<ClaimDocument>
            {
                Records = pagedDocuments,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        public async Task DeleteDocument(ClaimDocument document)
        {
            _context.ClaimDocument.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}