﻿using Microsoft.AspNetCore.Identity;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityUser : IdentityUser
    {
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

    }
}
