using CarGuideDDD.Core.EntityObjects.Interfaces;
using Riok.Mapperly.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityEndpointStatistics : IEntity
    {
        [Key]
        [MapperIgnore]
        public int Id { get; set; }
        public string? Endpoint { get; set; }
        public int VisitCount { get; set; }
    }
}
