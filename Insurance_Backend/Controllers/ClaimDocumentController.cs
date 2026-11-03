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
    public class ClaimDocumentController : BaseController
    {
        private readonly IClaimDocumentService _documentService;
        private readonly ICustomerProfileService _cusService;

        public ClaimDocumentController(IClaimDocumentService documentService, ICustomerProfileService cusService)
        {
            _documentService = documentService;
            _cusService = cusService;

        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> ListDocuments([FromQuery] PaginationQueryDTO query)
        {
            var documents = await _documentService.ListDocuments(query);
            return Success(documents, "Documents retrieved successfully");
        }

        [HttpGet("{documentId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)},{nameof(Role.Customer)}")]
        public async Task<IActionResult> GetDocumentById(int documentId)
        {
            var document = await _documentService.GetDocumentById(documentId);
            return Success(document, "Document retrieved successfully");

        }

        [HttpGet("claim/{claimId}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)},{nameof(Role.Customer)}")]
        public async Task<IActionResult> GetDocumentsByClaimId(int claimId)
        {
            var documents = await _documentService.GetDocumentsByClaimId(claimId);
            return Success(documents, "Documents retrieved successfully");

        }

        [HttpGet("customer/{customerId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetDocumentsByCustomerId(int customerId)
        {
            var documents = await _documentService.GetDocumentsByCustomerId(customerId);
            return Success(documents, "Documents retrieved successfully");

        }

        [HttpGet("my-documents")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = User.GetUserId();

            var documents = await _documentService.GetDocumentsByUserId(userId);

            return Success(documents, "Documents retrieved successfully");

        }

        [HttpPost]
        [Authorize(Roles = nameof(Role.Customer))]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddDocument([FromForm] ClaimDocumentRequestDTO request)
        {
            var documents = await _documentService.AddDocument(request, User);

            return Success(documents, "Document uploaded successfully.");
        }

        [HttpDelete("{documentId}")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            await _documentService.DeleteDocument(documentId, User);

            return Success<string?>(null, "Document deleted successfully."); //sending null becase of no data from the service 
        }
    }
}
