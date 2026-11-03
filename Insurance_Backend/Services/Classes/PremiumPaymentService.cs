
using Insurance_Backend.Config;
using Insurance_Backend.Custom;
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Insurance_Backend.Services.Classes
{
    public class PremiumPaymentService : IPremiumPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IPremiumPaymentRepository _paymentRepo;
        private readonly IPolicyRepository _policyRepo;
        private readonly ICustomerProfileRepository _customerRepo;
        private readonly ILogger<PremiumPaymentService> _logger;

        public PremiumPaymentService(
            AppDbContext context,
            IPremiumPaymentRepository paymentRepo,
            IPolicyRepository policyRepo,
            ICustomerProfileRepository customerRepo,
            ILogger<PremiumPaymentService> logger)
        {
            _context = context;
            _paymentRepo = paymentRepo;
            _policyRepo = policyRepo;
            _customerRepo = customerRepo;
            _logger = logger;
        }

        public async Task<PaginatedResponseDTO<PaymentResponseDTO>>
            GetAllPayments(PaginationQueryDTO query)
        {
            if (query == null)
                throw new BusinessException(
                    "Pagination query is required.",
                    StatusCodes.Status400BadRequest);

            var result = await _paymentRepo.GetAllPayments(query);

            return new PaginatedResponseDTO<PaymentResponseDTO>
            {
                Records = result.Records.Select(MapToResponse),
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        public async Task<PaymentResponseDTO?> GetPaymentById(int id)
        {
            if (id <= 0)
                throw new BusinessException(
                    "Payment ID must be greater than zero.",
                    StatusCodes.Status400BadRequest);

            var payment = await _paymentRepo.GetPaymentById(id);

            if (payment == null)
                throw new BusinessException(
                    "Payment not found.",
                    StatusCodes.Status404NotFound);

            return MapToResponse(payment);
        }

        public async Task<PaginatedResponseDTO<PaymentResponseDTO>>
            GetPaymentsByPolicyId(
                int policyId,
                PaginationQueryDTO query)
        {
            if (policyId <= 0)
                throw new BusinessException(
                    "Policy ID must be greater than zero.",
                    StatusCodes.Status400BadRequest);

            if (query == null)
                throw new BusinessException(
                    "Pagination query is required.",
                    StatusCodes.Status400BadRequest);

            var policyExists = await _context.Policy
                .AnyAsync(p => p.PolicyId == policyId);

            if (!policyExists)
                throw new BusinessException(
                    "Policy not found.",
                    StatusCodes.Status404NotFound);

            var result = await _paymentRepo.GetPaymentsByPolicyId(
                policyId, query);

            return new PaginatedResponseDTO<PaymentResponseDTO>
            {
                Records = result.Records.Select(MapToResponse),
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        public async Task<PaginatedResponseDTO<PaymentResponseDTO>>
            GetPaymentsByCustomerUserId(
                int userId,
                PaginationQueryDTO query)
        {
            if (userId <= 0)
                throw new BusinessException(
                    "Customer user ID must be greater than zero.",
                    StatusCodes.Status400BadRequest);

            if (query == null)
                throw new BusinessException(
                    "Pagination query is required.",
                    StatusCodes.Status400BadRequest);

            var result = await _paymentRepo
                .GetPaymentsByCustomerUserId(userId, query);

            return new PaginatedResponseDTO<PaymentResponseDTO>
            {
                Records = result.Records.Select(MapToResponse),
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        // Customer pays only for their own policy.
        public async Task<PaymentResponseDTO> RecordOwnPayment(
            PaymentRequestDTO request,
            ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                throw new BusinessException(
                    "Authentication is required.",
                    StatusCodes.Status401Unauthorized);

            var userId = user.GetUserId();

            var customer = await _customerRepo
                .GetCustomerProfileByUserId(userId);

            if (customer == null)
                throw new BusinessException(
                    "Customer profile does not exist.",
                    StatusCodes.Status404NotFound);

            if (customer.User == null ||
                customer.User.ActiveStatus != ActiveStatus.Active)
                throw new BusinessException(
                    "Customer is not active.",
                    StatusCodes.Status403Forbidden);

            var policy = await GetTrackedPolicy(request?.PolicyId ?? 0);

            if (policy.CustomerId != customer.CustomerId)
                throw new BusinessException(
                    "You can pay only for your own policies.",
                    StatusCodes.Status403Forbidden);

            return await RecordPayment(request!, policy);
        }

        // Admin/Officer records a payment on behalf of a customer.
        public async Task<PaymentResponseDTO> MakePayment(
            PaymentRequestDTO request)
        {
            if (request == null)
                throw new BusinessException(
                    "Payment request is required.",
                    StatusCodes.Status400BadRequest);

            var policy = await GetTrackedPolicy(request.PolicyId);

            return await RecordPayment(request, policy);
        }

        private async Task<Policy> GetTrackedPolicy(int policyId)
        {
            if (policyId <= 0)
                throw new BusinessException(
                    "Please select a valid policy.",
                    StatusCodes.Status400BadRequest);

            var policy = await _policyRepo.GetPolicyById(policyId);

            if (policy == null)
                throw new BusinessException(
                    "Policy does not exist.",
                    StatusCodes.Status404NotFound);

            if (policy.PolicyStatus == PolicyStatus.Cancelled ||
                policy.PolicyStatus == PolicyStatus.Expired)
                throw new BusinessException(
                    "Cannot make payment for a cancelled or expired policy.",
                    StatusCodes.Status400BadRequest);

            if (policy.PolicyPlan == null)
                throw new BusinessException(
                    "Policy plan details could not be found.",
                    StatusCodes.Status404NotFound);

            // A OneTime policy cannot receive another payment once active.
            if (policy.PolicyStatus == PolicyStatus.Active &&
                policy.PolicyPlan.PremiumType == PremiumType.OneTime)
                throw new BusinessException(
                    "This policy requires only one premium payment.",
                    StatusCodes.Status400BadRequest);

            // Validate the initial payment deadline.
            if (policy.PolicyStatus == PolicyStatus.PendingPayment &&
                DateTime.Today > policy.EndDate.Date)
                throw new BusinessException(
                    "The payment deadline has passed.",
                    StatusCodes.Status400BadRequest);

            return policy;
        }

        private async Task<PaymentResponseDTO> RecordPayment(
    PaymentRequestDTO request,
    Policy policy)
        {
            if (request == null)
                throw new BusinessException(
                    "Payment request is required.",
                    StatusCodes.Status400BadRequest);

            if (!Enum.IsDefined(typeof(PaymentMode), request.PaymentMode))
                throw new BusinessException(
                    "Invalid payment mode.",
                    StatusCodes.Status400BadRequest);

            if (policy.PolicyPlan == null)
                throw new BusinessException(
                    "Policy plan details could not be found.",
                    StatusCodes.Status404NotFound);

            var today = DateTime.Today;
            var plan = policy.PolicyPlan;
            var isInitialPayment =
                policy.PolicyStatus == PolicyStatus.PendingPayment;

            // The backend determines the installment amount.
            var amount = plan.PremiumAmount;

            // Validate initial payment deadline.
            if (isInitialPayment && today > policy.EndDate.Date)
                throw new BusinessException(
                    "The initial payment deadline has passed.",
                    StatusCodes.Status400BadRequest);

            // Validate recurring payment eligibility.
            if (!isInitialPayment)
            {
                if (policy.PolicyStatus != PolicyStatus.Active)
                    throw new BusinessException(
                        "This policy is not eligible for payment.",
                        StatusCodes.Status400BadRequest);

                if (plan.PremiumType == PremiumType.OneTime)
                    throw new BusinessException(
                        "This policy requires only one premium payment.",
                        StatusCodes.Status400BadRequest);

                if (!policy.DueDate.HasValue)
                    throw new BusinessException(
                        "The next premium due date is not configured.",
                        StatusCodes.Status400BadRequest);

                var dueDate = policy.DueDate.Value.Date;
                var paymentWindowStart = dueDate.AddDays(-5);

                if (today < paymentWindowStart || today > dueDate)
                    throw new BusinessException(
                        "Premium payment is allowed only within the five days before the due date.",
                        StatusCodes.Status400BadRequest);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // For OneTime policies, ensure no successful payment exists.
                if (plan.PremiumType == PremiumType.OneTime)
                {
                    var paymentExists = await _context.PremiumPayment
                        .AnyAsync(p =>
                            p.PolicyId == policy.PolicyId &&
                            p.PaymentStatus == PaymentStatus.Success);

                    if (paymentExists)
                        throw new BusinessException(
                            "This policy requires only one premium payment.",
                            StatusCodes.Status400BadRequest);
                }

                var paymentDate = DateTime.UtcNow;

                var payment = new PremiumPayment
                {
                    PolicyId = policy.PolicyId,
                    Amount = amount,
                    PaymentMode = request.PaymentMode,
                    TransactionReference = $"PAY-{Guid.NewGuid():N}",
                    PaymentDate = paymentDate,
                    CreatedDate = paymentDate,
                    PaymentStatus = PaymentStatus.Success
                };

                var createdPayment =
                    await _paymentRepo.AddPayment(payment);

                policy.TotalPremiumPaid += amount;

                if (isInitialPayment)
                {
                    // Coverage dates start from the successful initial payment date.
                    policy.StartDate = today;
                    policy.EndDate = today.AddYears(plan.Duration);

                    policy.DueDate = plan.PremiumType switch
                    {
                        PremiumType.OneTime => null,
                        PremiumType.Monthly => today.AddMonths(1),
                        PremiumType.Quarterly => today.AddMonths(3),
                        PremiumType.HalfYearly => today.AddMonths(6),
                        PremiumType.AnnualPremium => today.AddYears(1),
                        _ => throw new BusinessException(
                            "Invalid premium type.",
                            StatusCodes.Status400BadRequest)
                    };

                    policy.PolicyStatus = PolicyStatus.Active;
                }
                else
                {
                    // Advance from the scheduled due date, not the payment date.
                    var previousDueDate = policy.DueDate!.Value.Date;

                    policy.DueDate = plan.PremiumType switch
                    {
                        PremiumType.Monthly =>
                            previousDueDate.AddMonths(1),

                        PremiumType.Quarterly =>
                            previousDueDate.AddMonths(3),

                        PremiumType.HalfYearly =>
                            previousDueDate.AddMonths(6),

                        PremiumType.AnnualPremium =>
                            previousDueDate.AddYears(1),

                        _ => throw new BusinessException(
                            "Invalid recurring premium type.",
                            StatusCodes.Status400BadRequest)
                    };
                }

                policy.UpdatedDate = DateTime.UtcNow;

                await _policyRepo.SaveChangesAsync();
                await transaction.CommitAsync();

                createdPayment.Policy = policy;

                _logger.LogInformation(
                    "Payment {PaymentId} recorded for policy {PolicyNumber}. " +
                    "Amount: {Amount}, TransactionReference: {Reference}",
                    createdPayment.PaymentId,
                    policy.PolicyNumber,
                    amount,
                    payment.TransactionReference);

                return MapToResponse(createdPayment);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Concurrency conflict while recording payment for policy {PolicyId}.",
                    policy.PolicyId);

                throw new BusinessException(
                    "The policy was modified by another request. Please try again.",
                    StatusCodes.Status409Conflict);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                if (ex.GetBaseException() is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    throw new BusinessException(
                        "A duplicate transaction reference was detected. Please try again.",
                        StatusCodes.Status409Conflict);
                }

                _logger.LogError(
                    ex,
                    "Database error while recording payment for policy {PolicyId}.",
                    policy.PolicyId);

                throw;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static PaymentResponseDTO MapToResponse(
            PremiumPayment payment)
        {
            return new PaymentResponseDTO
            {
                PaymentId = payment.PaymentId,
                PolicyNumber = payment.Policy?.PolicyNumber ?? string.Empty,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMode = payment.PaymentMode,
                TransactionReference = payment.TransactionReference,
                PaymentStatus = payment.PaymentStatus
            };
        }
    }
}
