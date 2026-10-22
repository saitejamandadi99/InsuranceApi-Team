using AutoMapper;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Insurance_Backend.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDTO, User>();
            CreateMap<User, UserResponseDTO>();

            CreateMap<CustomerRequestDTO, CustomerProfile>();
            CreateMap<CustomerProfile, CustomerResponseDTO>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User!.FullName))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User!.Email))
                .ForMember(dest => dest.MobileNumber,
                    opt => opt.MapFrom(src => src.User!.MobileNumber));

            CreateMap<ProductRequestDTO, InsuranceProduct>();
            CreateMap<InsuranceProduct, ProductResponseDTO>();

            CreateMap<PlanRequestDTO, PolicyPlan>();
            CreateMap<PolicyPlan, PlanResponseDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.InsuranceProduct!.ProductName))
                .ForMember(dest => dest.ProductType,
                    opt => opt.MapFrom(src => src.InsuranceProduct!.ProductType));

            CreateMap<CustomerPolicyPurchaseRequestDTO, Policy>();
            CreateMap<OfficerOrAdminPolicyIssueRequestDTO, Policy>();
            CreateMap<Policy, PolicyResponseDTO>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer!.User!.FullName))
                .ForMember(dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.PolicyPlan!.PlanName))
                .ForMember(dest => dest.PremiumAmount,
                    opt => opt.MapFrom(src => src.PolicyPlan!.PremiumAmount));

            CreateMap<PaymentRequestDTO, PremiumPayment>();
            CreateMap<PremiumPayment, PaymentResponseDTO>()
                .ForMember(dest => dest.PolicyNumber,
                    opt => opt.MapFrom(src => src.Policy!.PolicyNumber));

            CreateMap<ClaimRequestDTO, Claim>();
            CreateMap<Claim, ClaimResponseDTO>()
                .ForMember(dest => dest.PolicyNumber,
                    opt => opt.MapFrom(src => src.Policy!.PolicyNumber))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Policy!.Customer!.User!.FullName));

            CreateMap<ClaimStatusHistory, ClaimStatusHistoryResponseDTO>()
                .ForMember(dest => dest.UpdatedBy,
                    opt => opt.MapFrom(src => src.User!.FullName))
                .ForMember(dest => dest.ClaimNumber,
                    opt => opt.MapFrom(src => src.Claim!.ClaimNumber));

            CreateMap<ClaimDocument, ClaimDocumentResponseDTO>()
                .ForMember(dest => dest.ClaimNumber,
                    opt => opt.MapFrom(src => src.Claim!.ClaimNumber));
        }
    }
}