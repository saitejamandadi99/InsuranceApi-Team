using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using System.Security.Claims;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IPremiumPaymentService
    {
        Task<PaginatedResponseDTO<PaymentResponseDTO>> GetAllPayments(
            PaginationQueryDTO query);

        Task<PaymentResponseDTO?> GetPaymentById(int id);

        Task<PaginatedResponseDTO<PaymentResponseDTO>> GetPaymentsByPolicyId(
            int policyId,
            PaginationQueryDTO query);

        Task<PaginatedResponseDTO<PaymentResponseDTO>> GetPaymentsByCustomerUserId(
            int userId,
            PaginationQueryDTO query);

        Task<PaymentResponseDTO> MakePayment(PaymentRequestDTO request);

        Task<PaymentResponseDTO> RecordOwnPayment(
    PaymentRequestDTO request,
    ClaimsPrincipal user);
    }
}