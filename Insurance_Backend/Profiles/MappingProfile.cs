using AutoMapper;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDTO, User>();
            CreateMap<User, UserResponseDTO>();

            CreateMap<ProductRequestDTO, InsuranceProduct>();
            CreateMap<InsuranceProduct, ProductResponseDTO>();
        }
    }
}
