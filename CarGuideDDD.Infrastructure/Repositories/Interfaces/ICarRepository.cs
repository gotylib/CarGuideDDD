﻿using Domain.Entities;
using DTOs;
using Infrastructure.Data;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface ICarRepository
    {
        Task<IEnumerable<PriorityCarDto>> GetAllAsync(bool priority);
        Task<PriorityCarDto> GetByIdAsync(int id);
        Task<bool> AddAsync(PriorityCarDto car);
        Task<bool> UpdateAsync(int id, PriorityCarDto car);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateQuantityAsync(int id, int quantity);
        Task<bool> SetAvailabilityAsync(int id, bool isAvailable);
    }
}
