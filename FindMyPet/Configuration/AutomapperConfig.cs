using AutoMapper;
using FindMyPet.Dto;
using FindMyPet.Dto.Login.Responses;
using FindMyPet.Models;

namespace FindMyPet.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
