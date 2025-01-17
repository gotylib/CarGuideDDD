

using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarGuideDDD.Infrastructure.Repositories
{
    public class ColorRepository : IColorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ColorRepository> _logger;
        public ColorRepository(ApplicationDbContext context, ILogger<ColorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<bool> AddAsync(ColorDto color)
        {
            try
            {
                var resultColor = await _context.Colors.FindAsync(color.Id);
                if (resultColor != null && resultColor.Color != color.Color)
                {
                    await _context.Colors.AddAsync(Maps.MapColorDtoToEntityColor(color));
                    await _context.SaveChangesAsync();
                    return true;
                }
                return true;

            }catch (Exception ex)
            {
                _logger.LogError("Еrror adding to the database", ex);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(IdDto id)
        {
            try
            {
                var color = await _context.Colors.FindAsync(id.Id);

                if(color == null) { return false; }

                _context.Colors.Remove(color);
                await _context.SaveChangesAsync();
                return true;

            }catch (Exception ex)
            {
                _logger.LogError("Еrror deleting to the database", ex);
                return false;
            }
        }

        public async  Task<ResultModel<IEnumerable<ColorDto?>, Exception>> GetAllAsync()
        {
            try
            {
                return ResultModel<IEnumerable<ColorDto?>,Exception>.CreateSuccessfulResult(
                        (await _context.Colors.ToListAsync())
                            .Select(Maps.MapEntityColoroColorDto));
            }catch (Exception ex)
            {
                return ResultModel<IEnumerable<ColorDto?>, Exception>.CreateFailedResult(ex);
            }
        }

        public async Task<bool> UpdateAsync(ColorDto color)
        {
            try
            {
                var updateColor = await _context.Colors.FindAsync(color.Id);

                if (updateColor == null) { return false; }

                updateColor.Color = color.Color;
                _context.Colors.Update(updateColor);
                await _context.SaveChangesAsync();
                
                return true;

            }catch(Exception ex)
            {
                _logger.LogError("Еrror updating to the database", ex);
                return false;
            }
        }
    }
}
