using AutoMapper;
using FindMyPet.Business.Models;
using FindMyPet.Dto.Responses;

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
