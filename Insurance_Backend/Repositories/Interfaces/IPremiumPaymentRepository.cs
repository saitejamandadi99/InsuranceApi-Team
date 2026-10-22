using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IPremiumPaymentRepository
    {
        Task<PaginatedResponseDTO<PremiumPayment>> GetAllPayments(
        PaginationQueryDTO query);
    Task<PremiumPayment?> GetPaymentById(int id);

        Task<PaginatedResponseDTO<PremiumPayment>> GetPaymentsByPolicyId(
            int policyId,
            PaginationQueryDTO query);

        Task<PaginatedResponseDTO<PremiumPayment>> GetPaymentsByCustomerUserId(
            int userId,
            PaginationQueryDTO query);

        Task<PremiumPayment> AddPayment(PremiumPayment payment);

        Task<PremiumPayment?> UpdatePaymentStatus(
            int id,
            PaymentStatus paymentStatus);

        Task<bool> PaymentExists(int id);

        Task<bool> TransactionReferenceExists(string transactionReference);
    }

}
