using CarGuideDDD.Core.EntityObjects.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityUser : IdentityUser, IEntity
    {
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

        public EntityBasket? Basket { get; set; }

        public string? SecretCode2FA { get; set;}

        public Guid? CodeFor2FA { get; set; }

    }
}
