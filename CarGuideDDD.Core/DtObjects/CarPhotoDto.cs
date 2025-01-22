using CarGuideDDD.Core.EntityObjects.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CarGuideDDD.Core.DtObjects
{
    public class CarPhotoDto : IEntity
    {
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? Color { get; init; }

        public IFormFile? file { get; init; }
    }
}
