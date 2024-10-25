﻿using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;


namespace CarGuideDDD.Infrastructure.Services
{
    public class StatisticService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsService;

        public StatisticService(IStatisticsRepository statisticsService)
        {
            _statisticsService = statisticsService;
        }
        public async Task<IEnumerable<EndpointStatisticsDto>> GetStatistics()
        {
            return await _statisticsService.GetStatisticAsync();
        }

        public async Task RecordVisit(string endpoint, string queryString)
        {
            var fullEndpoint = $"{endpoint}?{queryString}";

            await _statisticsService.AddOrConutStatisticsAsync(fullEndpoint);
        }
    }
}