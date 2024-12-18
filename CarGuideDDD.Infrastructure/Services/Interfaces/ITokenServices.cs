﻿using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(EntityUser user);
        RefreshTokenDto GenerateRefreshToken();

        Task<RefreshTokenDto> CreateRefreshTokenAsync(EntityUser user);

        Task<string> RefreshAccessTokenAsync(string refreshToken);

    }
}
