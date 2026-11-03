using Insurance_Backend.Config;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PremiumPaymentController : BaseController
    {
        private readonly IPremiumPaymentService _paymentService;

        public PremiumPaymentController(
            IPremiumPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("listOfPayments")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetAllPayments(
            [FromQuery] PaginationQueryDTO query)
        {
            var payments = await _paymentService.GetAllPayments(query);
            return Success(payments, "Payments retrieved successfully");
        }

        [HttpGet("{paymentId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetPaymentById(int paymentId)
        {
            var payment = await _paymentService.GetPaymentById(paymentId);
            return Success(payment, "Payment retrieved successfully");
        }

        [HttpGet("customer/{customerId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetPaymentsByCustomerId(
            int customerId,
            [FromQuery] PaginationQueryDTO query)
        {
            var payments =
                await _paymentService.GetPaymentsByCustomerUserId(
                    customerId, query);

            return Success(payments, "Customer payments retrieved successfully");
        }

        [HttpGet("policy/{policyId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetPaymentsByPolicyId(
            int policyId,
            [FromQuery] PaginationQueryDTO query)
        {
            var payments =
                await _paymentService.GetPaymentsByPolicyId(policyId, query);

            return Success(payments, "Policy payments retrieved successfully");
        }

        [HttpPost("officer-payment")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> MakeOfficerPayment(
            [FromBody] PaymentRequestDTO request)
        {
            var payment =
                await _paymentService.MakePayment(request);

            return Success(
                payment,
                "Payment recorded successfully by Admin/Officer");
        }

        [HttpPost("my-payment")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> MakeMyPayment(
            [FromBody] PaymentRequestDTO request)
        {
            var payment =
                await _paymentService.RecordOwnPayment(request, User);

            return Success(payment, "Payment recorded successfully");
        }

        [HttpGet("mypaymentsList")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> GetMyPayments(
            [FromQuery] PaginationQueryDTO query)
        {
            var userId = User.GetUserId();

            var payments =
                await _paymentService.GetPaymentsByCustomerUserId(
                    userId, query);

            return Success(payments, "Your payments retrieved successfully");
        }
    }
}
