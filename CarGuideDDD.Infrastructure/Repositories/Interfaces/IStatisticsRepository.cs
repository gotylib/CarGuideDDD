
using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IStatisticsRepository
    {
        Task AddOrConutStatisticsAsync(string fullEndpoint);
        Task<IEnumerable<EndpointStatisticsDto>> GetStatisticAsync();
    }
}
