using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task RecordVisit(string endpoint, string queryString);
       Task<IEnumerable<EndpointStatisticsDto>> GetStatistics();
    }
}
