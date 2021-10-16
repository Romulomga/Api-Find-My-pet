using AutoMapper;
using AdotaFacil.Api.Dto.Responses;
using AdotaFacil.Business.Models;

namespace AdotaFacil.Api.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
