

using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IColorRepository
    {
        Task<bool> AddAsync(ColorDto color);
        Task<bool> UpdateAsync(ColorDto color);
        Task<bool> DeleteAsync(IdDto id);
        Task<ResultModel<IEnumerable<ColorDto?>, Exception>> GetAllAsync(); 
    }
}
