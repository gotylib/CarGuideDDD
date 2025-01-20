

using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public class DeleteCarFromBasketDto : CDUBasketDto
    {
        public int CarId { get; set; }  
    }
}
