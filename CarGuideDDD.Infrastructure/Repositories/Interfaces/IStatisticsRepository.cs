using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IStatisticsRepository
    {
        Task AddOrCountStatisticsAsync(string fullEndpoint);
        Task<IEnumerable<EndpointStatisticsDto>> GetStatisticAsync();
    }
}
