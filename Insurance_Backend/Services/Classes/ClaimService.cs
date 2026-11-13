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
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly ICustomerProfileRepository _customerRepo;
        private readonly IClaimDocumentRepository _documentRepo;
        private readonly IClaimStatusHistoryRepository _historyRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ClaimService> _logger;
        private readonly ICloudDocumentService _docService;

        public ClaimService(
            IClaimRepository claimRepo,
            IPolicyRepository policyRepo,
            ICustomerProfileRepository customerRepo,
            IClaimDocumentRepository documentRepo,
            IClaimStatusHistoryRepository historyRepo,
            IMapper mapper,
            ILogger<ClaimService> logger,
            ICloudDocumentService docService)
        {
            _claimRepo = claimRepo;
            _policyRepo = policyRepo;
            _customerRepo = customerRepo;
            _documentRepo = documentRepo;
            _historyRepo = historyRepo;
            _mapper = mapper;
            _logger = logger;
            _docService = docService;
        }

        // Raise a new claim
        public async Task<ClaimResponseDTO> RaiseClaim(
            ClaimRequestDTO request,
            ClaimsPrincipal user)
        {
            var userId = user.GetUserId();

            var customer = await _customerRepo
                .GetCustomerProfileByUserId(userId);

            if (customer == null)
            {
                throw new BusinessException(
                    "Customer does not exist.", 404);
            }

            if (customer.User == null ||
                customer.User.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Customer is not active.", 400);
            }

            var policy = await _policyRepo
                .GetPolicyById(request.PolicyId);

            if (policy == null)
            {
                throw new BusinessException(
                    "Policy does not exist.", 404);
            }

            // Customers can raise claims only against their own policies.
            if (policy.CustomerId != customer.CustomerId)
            {
                throw new BusinessException(
                    "You can raise claims only for your own policies.", 403);
            }

            if (policy.PolicyStatus != PolicyStatus.Active)
            {
                throw new BusinessException(
                    "Claim can be raised only for active policies.", 400);
            }

            if (policy.PolicyPlan == null)
            {
                throw new BusinessException(
                    "Policy plan does not exist.", 404);
            }

            if (request.ClaimAmount <= 0)
            {
                throw new BusinessException(
                    "Claim amount should be greater than 0.", 400);
            }

            // Claim amount cannot exceed the plan's coverage.
            if (request.ClaimAmount >
                policy.PolicyPlan.CoverageAmount)
            {
                throw new BusinessException(
                    "Claim amount exceeds policy coverage amount.", 400);
            }

            // Include existing reserved claims when checking available coverage.
            var reservedAmount = await _claimRepo
                .GetReserveAmountByPolicyId(policy.PolicyId);

            var availableAmount =
                policy.PolicyPlan.CoverageAmount - reservedAmount;

            if (request.ClaimAmount > availableAmount)
            {
                throw new BusinessException(
                    $"Only {availableAmount:C} coverage is available for this policy.",
                    400);
            }

            // Customer can raise a claim only after completing 20 days.
            var claimEligibleDate = policy.StartDate.Date.AddDays(20);

            if (DateTime.Today < claimEligibleDate)
            {
                throw new BusinessException(
                    $"You can raise a claim only after 20 days from the policy start date. You can raise a claim from {claimEligibleDate:dd MMM yyyy}.",
                    400);
            }

            if (request.IncidentDate > DateTime.Now)
            {
                throw new BusinessException(
                    "Incident date cannot be a future date.", 400);
            }

            if (request.IncidentDate < policy.StartDate)
            {
                throw new BusinessException(
                    "Incident date cannot be before the policy start date.",
                    400);
            }

            if (string.IsNullOrWhiteSpace(request.ClaimReason))
            {
                throw new BusinessException(
                    "Claim reason is required.", 400);
            }

            if (request.Files == null ||
                request.Files.Count == 0 ||
                request.Files.Any(file =>
                    file == null || file.Length == 0))
            {
                throw new BusinessException(
                    "At least one valid document is required.", 400);
            }

            var claim = new Insurance_Backend.Models.Claim
            {
                ClaimNumber = await GenerateClaimNumber(),
                PolicyId = request.PolicyId,
                ClaimAmount = request.ClaimAmount,
                ClaimReason = request.ClaimReason.Trim(),
                IncidentDate = request.IncidentDate,
                ClaimStatus = ClaimStatus.Pending,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var createdClaim = await _claimRepo.CreateClaim(claim);

            // Upload and save supporting documents.
            foreach (var file in request.Files)
            {
                var documentReference =
                    await _docService.UploadClaimDocumentAsync(file);

                var document = new ClaimDocument
                {
                    ClaimId = createdClaim.ClaimId,
                    DocumentName = "Supporting Document",
                    DocumentType = "Reference",
                    DocumentReference = documentReference
                };

                await _documentRepo.AddClaimDocument(document);
            }

            // Record the initial claim status.
            var history = new ClaimStatusHistory
            {
                ClaimId = createdClaim.ClaimId,
                PreviousStatus = ClaimStatus.Pending,
                NewStatus = ClaimStatus.Pending,
                Remarks = "Claim Raised",
                UpdatedBy = userId,
                UpdatedDate = DateTime.Now
            };

            await _historyRepo.AddHistory(history);

            // Supply navigation data required for response mapping.
            createdClaim.Policy = policy;

            _logger.LogInformation(
                "Claim '{ClaimNumber}' raised for policy '{PolicyNumber}'.",
                createdClaim.ClaimNumber,
                policy.PolicyNumber);

            return await MapClaimResponse(createdClaim);
        }

        // Generate a unique claim number
        public async Task<string> GenerateClaimNumber()
        {
            string claimNumber;

            do
            {
                claimNumber = Random.Shared
                    .NextInt64(1000000000, 9999999999)
                    .ToString();
            }
            while (
                await _claimRepo.GetClaimByClaimNumber(claimNumber) != null
            );

            return claimNumber;
        }

        // Officer reviews a claim
        public async Task<ClaimResponseDTO> OfficerReview(
            int claimId,
            OfficerRemarkDTO request,
            ClaimsPrincipal user)
        {
            var userId = user.GetUserId();
            var newStatus = request.ClaimStatus;

            var claim = await _claimRepo.GetClaimById(claimId);

            if (claim == null)
            {
                throw new BusinessException(
                    "Claim does not exist.", 404);
            }

            if (claim.ClaimStatus != ClaimStatus.Pending)
            {
                throw new BusinessException(
                    "Only pending claims can be reviewed by an officer.",
                    409);
            }

            if (newStatus != ClaimStatus.RecommendedForApproval &&
                newStatus != ClaimStatus.RecommendedForRejection)
            {
                throw new BusinessException(
                    "Officer can only recommend to approve or reject.",
                    400);
            }

            return await UpdateClaimStatus(
                claimId,
                newStatus,
                request.OfficerRemarks,
                true,
                userId);
        }

        // Admin reviews a claim
        public async Task<ClaimResponseDTO> AdminReview(
            int claimId,
            AdminRemarkDTO request,
            ClaimsPrincipal user)
        {
            var userId = user.GetUserId();

            var claim = await _claimRepo.GetClaimById(claimId);

            if (claim == null)
            {
                throw new BusinessException(
                    "Claim does not exist.", 404);
            }

            var newStatus = request.ClaimStatus;

            if (newStatus != ClaimStatus.Approved &&
                newStatus != ClaimStatus.Rejected)
            {
                throw new BusinessException(
                    "Admin can only approve or reject claims.", 400);
            }

            // Officer must review the claim before admin action.
            if (claim.ClaimStatus != ClaimStatus.RecommendedForApproval &&
                claim.ClaimStatus != ClaimStatus.RecommendedForRejection)
            {
                throw new BusinessException(
                    "Claim must be reviewed by an officer before admin action.",
                    409);
            }

            if (claim.ClaimStatus == ClaimStatus.RecommendedForRejection &&
                newStatus != ClaimStatus.Rejected)
            {
                throw new BusinessException(
                    "Claims recommended for rejection can only be rejected.",
                    409);
            }

            return await UpdateClaimStatus(
                claimId,
                newStatus,
                request.AdminRemarks,
                false,
                userId);
        }

        // Get claim by ID
        public async Task<ClaimResponseDTO?> GetClaimById(int claimId)
        {
            var claim = await _claimRepo.GetClaimById(claimId);

            if (claim == null)
            {
                throw new BusinessException(
                    "Claim does not exist.", 404);
            }

            return await MapClaimResponse(claim);
        }

        // Get claims by customer ID
        public async Task<IEnumerable<ClaimResponseDTO>>
            GetClaimsByCustomerId(int customerId)
        {
            var claims = await _claimRepo
                .GetClaimsByCustomerId(customerId);

            return await MapClaimResponses(claims);
        }

        // Get claims by policy ID
        public async Task<IEnumerable<ClaimResponseDTO>>
            GetClaimsByPolicyId(int policyId)
        {
            var claims = await _claimRepo
                .GetClaimsByPolicyId(policyId);

            return await MapClaimResponses(claims);
        }

        // Get paginated claims
        public async Task<PaginatedResponseDTO<ClaimResponseDTO>>
            ListClaims(PaginationQueryDTO query)
        {
            var claims = await _claimRepo.ListClaims(query);

            var records = await MapClaimResponses(claims.Records);

            return new PaginatedResponseDTO<ClaimResponseDTO>
            {
                Records = records,
                CurrentPage = claims.CurrentPage,
                PageSize = claims.PageSize,
                TotalRecords = claims.TotalRecords,
                TotalPages = claims.TotalPages,
                IsLastPage = claims.IsLastPage,
                SortBy = claims.SortBy,
                SortDirection = claims.SortDirection
            };
        }

        // Update claim status
        private async Task<ClaimResponseDTO> UpdateClaimStatus(
            int claimId,
            ClaimStatus newStatus,
            string remarks,
            bool isOfficer,
            int updatedBy)
        {
            var claim = await _claimRepo.GetClaimById(claimId);

            if (claim == null)
            {
                throw new BusinessException(
                    "Claim does not exist.", 404);
            }

            if (claim.ClaimStatus == ClaimStatus.Approved ||
                claim.ClaimStatus == ClaimStatus.Rejected)
            {
                throw new BusinessException(
                    "Approved or rejected claims cannot be modified.",
                    409);
            }

            var previousStatus = claim.ClaimStatus;

            claim.ClaimStatus = newStatus;

            if (isOfficer)
            {
                claim.OfficerRemarks = remarks;
            }
            else
            {
                claim.AdminRemarks = remarks;
            }

            await _claimRepo.UpdateClaim(claim);

            // Record claim status history.
            var history = new ClaimStatusHistory
            {
                ClaimId = claim.ClaimId,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Remarks = remarks,
                UpdatedBy = updatedBy,
                UpdatedDate = DateTime.Now
            };

            await _historyRepo.AddHistory(history);

            return await MapClaimResponse(claim);
        }

        // Get claims by user ID
        public async Task<IEnumerable<ClaimResponseDTO>>
            GetClaimsByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            var claims = await _claimRepo
                .GetClaimsByUserId(userId);

            return await MapClaimResponses(claims);
        }

        // Map a single claim response
        private async Task<ClaimResponseDTO> MapClaimResponse(
            Insurance_Backend.Models.Claim claim)
        {
            var response = _mapper.Map<ClaimResponseDTO>(claim);

            // Retrieve claim history to identify the officer.
            var history = await _historyRepo
                .GetCompleteHistoryByClaimId(claim.ClaimId);

            // Find the latest officer recommendation.
            var officerHistory = history
                .Where(h =>
                    h.NewStatus == ClaimStatus.RecommendedForApproval ||
                    h.NewStatus == ClaimStatus.RecommendedForRejection)
                .OrderByDescending(h => h.UpdatedDate)
                .FirstOrDefault();

            response.UpdatedByName =
                officerHistory?.User?.FullName ?? "Not reviewed yet";

            return response;
        }

        // Map multiple claim responses
        private async Task<IEnumerable<ClaimResponseDTO>> MapClaimResponses(
            IEnumerable<Insurance_Backend.Models.Claim> claims)
        {
            var claimList = claims.ToList();

            var responses = new List<ClaimResponseDTO>();

            foreach (var claim in claimList)
            {
                var response = await MapClaimResponse(claim);
                responses.Add(response);
            }

            return responses;
        }
    }
}