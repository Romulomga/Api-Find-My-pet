using System;

namespace AdotaFacil.Api.Dto.Responses
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
