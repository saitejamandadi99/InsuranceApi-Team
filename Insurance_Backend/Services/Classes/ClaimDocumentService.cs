using AutoMapper;
using Insurance_Backend.Config;
using Insurance_Backend.Custom;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using System.Security.Claims;

namespace Insurance_Backend.Services.Classes
{
    public class ClaimDocumentService:IClaimDocumentService
    {
        private readonly IClaimDocumentRepository _documentRepo;
        private readonly IClaimRepository _claimRepo;
        private readonly ICustomerProfileRepository _cusRepo;
        private readonly ICloudDocumentService _docService;
        private readonly IMapper _mapper;

        public ClaimDocumentService(IClaimDocumentRepository documentRepo, IMapper mapper, IClaimRepository claimRepo, ICustomerProfileRepository cusRepo, ICloudDocumentService docService)
        {
            _documentRepo = documentRepo;
            _mapper = mapper;
            _claimRepo = claimRepo;
            _cusRepo = cusRepo;
            _docService = docService;
        }
        public async Task<PaginatedResponseDTO<ClaimDocumentResponseDTO>> ListDocuments(PaginationQueryDTO query)
        {
            var documents = await _documentRepo.ListDocuments(query);

            return new PaginatedResponseDTO<ClaimDocumentResponseDTO>
            {
                Records = _mapper.Map<IEnumerable<ClaimDocumentResponseDTO>>(documents.Records),
                CurrentPage = documents.CurrentPage,
                PageSize = documents.PageSize,
                TotalRecords = documents.TotalRecords,
                TotalPages = documents.TotalPages,
                IsLastPage = documents.IsLastPage,
                SortBy = documents.SortBy,
                SortDirection = documents.SortDirection
            };
        }

        public async Task<ClaimDocumentResponseDTO?> GetDocumentById(int documentId)
        {
            var document = await _documentRepo.GetDocumentById(documentId);
            return _mapper.Map<ClaimDocumentResponseDTO>(document);
        }

        public async Task<IEnumerable<ClaimDocumentResponseDTO>> GetDocumentsByClaimId(int claimId)
        {
            var documents = await _documentRepo.GetDocumentsByClaimId(claimId);

            return _mapper.Map<IEnumerable<ClaimDocumentResponseDTO>>(documents);
        }

        public async Task<IEnumerable<ClaimDocumentResponseDTO>> GetDocumentsByCustomerId(int customerId)
        {
            var documents = await _documentRepo.GetDocumentsByCustomerId(customerId);
            return _mapper.Map<IEnumerable<ClaimDocumentResponseDTO>>(documents);
        }

        public async Task<IEnumerable<ClaimDocumentResponseDTO>> AddDocument(ClaimDocumentRequestDTO request, ClaimsPrincipal user)
        {
            var existingClaim = await _claimRepo.GetClaimById(request.ClaimId);
            if (existingClaim == null)
            {
                throw new Exception("Claim does not exists");
            }
            var customer = await _cusRepo.GetCustomerProfileByUserId(user.GetUserId());
            if (customer == null)
            {
                throw new Exception("customer does not exists");

            }
            if (customer.User.ActiveStatus != ActiveStatus.Active)
            {
                throw new Exception("customer is not active");

            }
            if (existingClaim.Policy.CustomerId != customer.CustomerId)
            {
                throw new Exception("You can upload documents only for your own claims.");

            }
            if (existingClaim.ClaimStatus == ClaimStatus.Approved || existingClaim.ClaimStatus == ClaimStatus.Rejected)
            {
                throw new Exception("Documents cannot be added after claim is finalized.");
            }

            if (request.Files == null || request.Files.Count == 0)
            {
                throw new Exception("At least one file is required");
            }

            var createdDocs = new List<ClaimDocument>();

            foreach (var file in request.Files)
            {
                if (file.Length == 0)
                {
                    continue;
                }
                var documentReference = await _docService.UploadClaimDocumentAsync(file);

                var doc = new ClaimDocument
                {
                    ClaimId = request.ClaimId,
                    DocumentName = request.DocumentName,
                    DocumentType = request.DocumentType,
                    DocumentReference = documentReference,
                    UploadedDate = DateTime.Now
                };

                var created = await _documentRepo.AddClaimDocument(doc);
                createdDocs.Add(created);
            }

            return _mapper.Map<IEnumerable<ClaimDocumentResponseDTO>>(createdDocs);

        }

        public async Task DeleteDocument(int documentId, ClaimsPrincipal user)
        {
            var document = await _documentRepo.GetDocumentById(documentId);

            if (document == null)
            {
                throw new Exception("Document does not exist.");
            }


            var customer = await _cusRepo.GetCustomerProfileByUserId(user.GetUserId());
            if (customer == null)
            {
                throw new Exception("customer does not exists");

            }
            if (customer.User.ActiveStatus != ActiveStatus.Active)
            {
                throw new Exception("customer is not active");

            }
            if (document.Claim.Policy.CustomerId != customer.CustomerId)
            {
                throw new Exception("You can delete documents only for your own claims.");

            }

            if (document.Claim.ClaimStatus == ClaimStatus.Approved || document.Claim.ClaimStatus == ClaimStatus.Rejected)
            {
                throw new Exception("Documents cannot be deleted after claim is finalized.");
            }
            await _docService.DeleteDocumentAsync(document.DocumentReference);

            await _documentRepo.DeleteDocument(document);

        }


        public async Task<IEnumerable<ClaimDocumentResponseDTO>>
            GetDocumentsByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            var documents = await _documentRepo.GetDocumentsByUserId(userId);

            return _mapper.Map<IEnumerable<ClaimDocumentResponseDTO>>(documents);
        }

    }
}
