
using CarGuideDDD.Core.EntityObjects.Interfaces;

namespace CarGuideDDD.Core.DtObjects
{
    public class TokenDto : IEntity
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

    }
}
