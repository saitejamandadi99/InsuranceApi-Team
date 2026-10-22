using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class PremiumPaymentRepository : IPremiumPaymentRepository
    {
        private readonly AppDbContext _context;

    public PremiumPaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reusable entity projection with related Policy details
        private IQueryable<PremiumPayment> GetPaymentQuery()
        {
            return _context.PremiumPayment
                .AsNoTracking()
                .Select(p => new PremiumPayment
                {
                    PaymentId = p.PaymentId,
                    PolicyId = p.PolicyId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMode = p.PaymentMode,
                    TransactionReference = p.TransactionReference,
                    PaymentStatus = p.PaymentStatus,
                    CreatedDate = p.CreatedDate,

                    Policy = p.Policy == null
                        ? null
                        : new Policy
                        {
                            PolicyId = p.Policy.PolicyId,
                            PolicyNumber = p.Policy.PolicyNumber,
                            CustomerId = p.Policy.CustomerId,
                            PlanId = p.Policy.PlanId,
                            StartDate = p.Policy.StartDate,
                            EndDate = p.Policy.EndDate,
                            PolicyStatus = p.Policy.PolicyStatus,
                            TotalPremiumPaid = p.Policy.TotalPremiumPaid,
                            CreatedDate = p.Policy.CreatedDate,
                            UpdatedDate = p.Policy.UpdatedDate,

                            Customer = p.Policy.Customer == null
                                ? null
                                : new CustomerProfile
                                {
                                    User = p.Policy.Customer.User == null
                                        ? null
                                        : new User
                                        {
                                            FullName =
                                                p.Policy.Customer.User.FullName
                                        }
                                },

                            PolicyPlan = p.Policy.PolicyPlan == null
                                ? null
                                : new PolicyPlan
                                {
                                    PlanName =
                                        p.Policy.PolicyPlan.PlanName,
                                    PremiumAmount =
                                        p.Policy.PolicyPlan.PremiumAmount
                                }
                        }
                });
        }

        // Get All Payments
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetAllPayments(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<PremiumPayment> payments = GetPaymentQuery();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                payments = payments.Where(p =>
                    p.TransactionReference.Contains(searchTerm));
            }

            // Sorting
            payments = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("amount", "desc") =>
                    payments.OrderByDescending(p => p.Amount)
                            .ThenBy(p => p.PaymentId),

                ("amount", _) =>
                    payments.OrderBy(p => p.Amount)
                            .ThenBy(p => p.PaymentId),

                ("paymentdate", "desc") =>
                    payments.OrderByDescending(p => p.PaymentDate)
                            .ThenBy(p => p.PaymentId),

                ("paymentdate", _) =>
                    payments.OrderBy(p => p.PaymentDate)
                            .ThenBy(p => p.PaymentId),

                _ => payments.OrderBy(p => p.PaymentId)
            };

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedPayments = await payments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = pagedPayments,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get Payment By ID
        public async Task<PremiumPayment?> GetPaymentById(int id)
        {
            return await GetPaymentQuery()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Get Payments By Policy ID
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetPaymentsByPolicyId(
                int policyId,
                PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            var payments = GetPaymentQuery()
                .Where(p => p.PolicyId == policyId);

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedPayments = await payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = pagedPayments,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get Payments By Customer User ID
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetPaymentsByCustomerUserId(
                int userId,
                PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            var payments = GetPaymentQuery()
                .Where(p =>
                    p.Policy != null &&
                    p.Policy.Customer != null &&
                    p.Policy.Customer.UserId == userId);

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedPayments = await payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = pagedPayments,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Add Payment
        public async Task<PremiumPayment> AddPayment(
            PremiumPayment payment)
        {
            if (payment.Amount <= 0)
            {
                throw new ArgumentException(
                    "Payment amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(payment.TransactionReference))
            {
                throw new ArgumentException(
                    "Transaction reference is required.");
            }

            if (await TransactionReferenceExists(
                payment.TransactionReference))
            {
                throw new InvalidOperationException(
                    "Transaction reference already exists.");
            }

            bool policyExists = await _context.Policy
                .AnyAsync(p => p.PolicyId == payment.PolicyId);

            if (!policyExists)
            {
                throw new KeyNotFoundException("Policy not found.");
            }

            // New payment starts as pending.
            payment.PaymentStatus = PaymentStatus.Pending;
            payment.CreatedDate = DateTime.UtcNow;

            _context.PremiumPayment.Add(payment);

            await _context.SaveChangesAsync();

            return await GetPaymentQuery()
                .FirstAsync(p => p.PaymentId == payment.PaymentId);
        }

        // Update Payment Status
        public async Task<PremiumPayment?> UpdatePaymentStatus(
            int id,
            PaymentStatus paymentStatus)
        {
            var payment = await _context.PremiumPayment
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return null;
            }

            if (payment.PaymentStatus != PaymentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending payments can be updated.");
            }

            if (paymentStatus != PaymentStatus.Success &&
                paymentStatus != PaymentStatus.Failed)
            {
                throw new ArgumentException(
                    "Payment status must be Success or Failed.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                payment.PaymentStatus = paymentStatus;

                // Only successful payments update the policy.
                if (paymentStatus == PaymentStatus.Success)
                {
                    var policy = await _context.Policy
                        .Include(p => p.PolicyPlan)
                        .FirstOrDefaultAsync(
                            p => p.PolicyId == payment.PolicyId);

                    if (policy == null)
                    {
                        throw new KeyNotFoundException(
                            "Associated policy not found.");
                    }

                    policy.TotalPremiumPaid += payment.Amount;
                    policy.UpdatedDate = DateTime.UtcNow;

                    // Activate once the required initial premium is paid.
                    if (policy.PolicyPlan != null &&
                        policy.TotalPremiumPaid >=
                        policy.PolicyPlan.PremiumAmount)
                    {
                        policy.PolicyStatus = PolicyStatus.Active;
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetPaymentQuery()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Check Payment Exists
        public async Task<bool> PaymentExists(int id)
        {
            return await _context.PremiumPayment
                .AnyAsync(p => p.PaymentId == id);
        }

        // Check Transaction Reference Exists
        public async Task<bool> TransactionReferenceExists(
            string transactionReference)
        {
            return await _context.PremiumPayment
                .AnyAsync(p =>
                    p.TransactionReference == transactionReference);
        }
    }

}
