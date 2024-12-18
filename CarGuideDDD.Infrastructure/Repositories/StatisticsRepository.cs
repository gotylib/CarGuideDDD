﻿using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using Microsoft.EntityFrameworkCore;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;

namespace CarGuideDDD.Infrastructure.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly ApplicationDbContext _context;

        public StatisticsRepository(ApplicationDbContext context) 
        {
            _context = context; 
        }
        public async Task AddOrCountStatisticsAsync(string fullEndpoint)
        {

            var stat = await _context.EndpointStatistics.FirstOrDefaultAsync(s =>
            s.Endpoint == fullEndpoint);
            if(stat != null)
            {
                stat.VisitCount++;
                 _context.EndpointStatistics.Update(stat);
            }
            else
            {
                await _context.EndpointStatistics.AddAsync(new EntityEndpointStatistics {Endpoint = fullEndpoint, VisitCount = 1 });  
            }
            await _context.SaveChangesAsync();

        }

        public async Task<IEnumerable<EndpointStatisticsDto>> GetStatisticAsync()
        {
            return (await _context.EndpointStatistics.ToListAsync()).Select(Maps.MapEntityEndpointToEndpointDto).OrderByDescending(dto => dto.VisitCount);
        }
    }
}
