using CarGuideDDD.Core.EntityObjects.Interfaces;

namespace CarGuideDDD.Core.DtObjects
{
    public class RefreshTokenDto : IEntity
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
