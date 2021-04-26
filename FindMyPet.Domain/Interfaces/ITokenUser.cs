using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace FindMyPet.Business.Interfaces
{
    public interface ITokenUser
    {
        string Name { get; }
        long? GetUserId();
        string? GetUserEmail();
        bool IsAuthenticated();
    }
}
