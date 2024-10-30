namespace CarGuideDDD.Core.DtObjects
{
    public class PriorityCarDto
    {
        public int Id { get; init; }
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? Color { get; init; }
        public int StockCount { get; init; }
        public bool IsAvailable { get; init; }
    }
}
