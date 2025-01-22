using CarGuideDDD.Core.EntityObjects.Interfaces;

namespace CarGuideDDD.Core.DtObjects
{
    public class PriorityCarDto : IEntity
    {
        public int Id { get; init; }
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? Color { get; init; }
        public DateTime AddTime { get; set; }
        public string? AddUserName { get; set; } // Будет доступно для просмотра только админу, менджеру
        public string? NameOfPhoto { get; set; }
        public int StockCount { get; init; }
        public bool IsAvailable { get; init; }
    }
}
