using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface ICarRepository
    {
        IQueryable<PriorityCarDto> GetAll(bool priority);
        Task<PriorityCarDto> GetByIdAsync(int id);
        Task<bool> AddAsync(PriorityCarDto car);
        Task<bool> UpdateAsync(int id, PriorityCarDto car);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateQuantityAsync(int id, int quantity);
        Task<bool> SetAvailabilityAsync(int id, bool isAvailable);
    }
}
