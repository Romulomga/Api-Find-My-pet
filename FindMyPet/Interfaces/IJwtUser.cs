using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace FindMyPet.Interfaces
{
    public interface IJwtUser
    {
        Guid GetUserId();
    }
}
