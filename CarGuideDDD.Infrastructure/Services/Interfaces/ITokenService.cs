
using Domain.Entities;
using DTOs;
using Infrastructure.Data;


namespace Domain.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(EntityUser user);
        RefreshTokenDto GenerateRefreshToken();
    }
}
