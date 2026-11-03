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

        // Reusable payment projection.
        // Fetch related details using Select()
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
                                    UserId = p.Policy.Customer.UserId,

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
                                    PlanId = p.Policy.PolicyPlan.PlanId,
                                    PlanName =
                                        p.Policy.PolicyPlan.PlanName,
                                    PremiumAmount =
                                        p.Policy.PolicyPlan.PremiumAmount
                                }
                        }
                });
        }

        // Get all payments with pagination, search, and sorting.
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetAllPayments(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<PremiumPayment> payments = GetPaymentQuery();

            // Search by transaction reference.
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                payments = payments.Where(p =>
                    p.TransactionReference != null &&
                    p.TransactionReference.Contains(searchTerm));
            }

            // Sorting.
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

                ("status", "desc") =>
                    payments.OrderByDescending(p => p.PaymentStatus)
                            .ThenBy(p => p.PaymentId),

                ("status", _) =>
                    payments.OrderBy(p => p.PaymentStatus)
                            .ThenBy(p => p.PaymentId),

                _ => payments.OrderBy(p => p.PaymentId)
            };

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var records = await payments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = records,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get payment by ID.
        public async Task<PremiumPayment?> GetPaymentById(int id)
        {
            return await GetPaymentQuery()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Get payments by policy ID.
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetPaymentsByPolicyId(
                int policyId,
                PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            var payments = GetPaymentQuery()
                .Where(p => p.PolicyId == policyId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId);

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var records = await payments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = records,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Get payment history for a customer.
        public async Task<PaginatedResponseDTO<PremiumPayment>>
            GetPaymentsByCustomerUserId(
                int userId,
                PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            // Filter the original entities by the authenticated customer's UserId.
            var payments = _context.PremiumPayment
                .AsNoTracking()
                .Where(p => p.Policy.Customer.UserId == userId);

            int totalRecords = await payments.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            // Apply ordering and pagination before projecting related details.
            var records = await payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                                    UserId = p.Policy.Customer.UserId,

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
                                    PlanId = p.Policy.PolicyPlan.PlanId,
                                    PlanName =
                                        p.Policy.PolicyPlan.PlanName,
                                    PremiumAmount =
                                        p.Policy.PolicyPlan.PremiumAmount
                                }
                        }
                })
                .ToListAsync();

            return new PaginatedResponseDTO<PremiumPayment>
            {
                Records = records,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Add payment.

        public async Task<PremiumPayment> AddPayment(
            PremiumPayment payment)
        {
            _context.PremiumPayment.Add(payment);

            await _context.SaveChangesAsync();

            return await GetPaymentQuery()
                .FirstAsync(p => p.PaymentId == payment.PaymentId);
        }

        // Update payment status only.
        
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

            payment.PaymentStatus = paymentStatus;

            await _context.SaveChangesAsync();

            return await GetPaymentQuery()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        // Check whether a payment exists.
        public async Task<bool> PaymentExists(int id)
        {
            return await _context.PremiumPayment
                .AnyAsync(p => p.PaymentId == id);
        }

        // Check whether a transaction reference exists.
        public async Task<bool> TransactionReferenceExists(
            string transactionReference)
        {
            return await _context.PremiumPayment
                .AnyAsync(p =>
                    p.TransactionReference == transactionReference);
        }

        // Check whether a policy exists.
        public async Task<bool> PolicyExists(int policyId)
        {
            return await _context.Policy
                .AnyAsync(p => p.PolicyId == policyId);
        }
    }
}