using CarGuideDDD.Core.EntityObjects.Interfaces;

namespace CarGuideDDD.Core.DtObjects
{
    public class UserDto : IEntity
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public string? SecretCode { get; set; }  

    }
}
