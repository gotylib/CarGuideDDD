using DTOs;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(EntityUser user);
        RefreshTokenDto GenerateRefreshToken();
    }
}
