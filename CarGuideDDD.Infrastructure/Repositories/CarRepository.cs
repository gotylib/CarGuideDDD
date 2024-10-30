using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;


namespace CarGuideDDD.Infrastructure.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public CarRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<bool> AddAsync(PriorityCarDto car)
        {

            await _context.Cars.AddAsync(Maps.MapPriorityCarDtoToEntityCar(car));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var car = await GetByIdAsync(id);
            _context.Cars.Remove(Maps.MapPriorityCarDtoToEntityCar(car));
            await _context.SaveChangesAsync();
            return true;

        }

        public IQueryable<PriorityCarDto> GetAll(bool priority)
        {
            var query = _context.Cars.AsQueryable();

            if (priority)
            {
                // Если приоритет, возвращаем все автомобили
                return query.Select(car => new PriorityCarDto
                {
                    Id = car.Id,
                    Make = car.Make,
                    Model = car.Model,
                    Color = car.Color,
                    StockCount = car.StockCount,
                    IsAvailable = car.IsAvailable
                });
            }
            
            // Если не приоритет, фильтруем по доступности
            return query.Where(c => c.IsAvailable)
                        .Select(car => new PriorityCarDto
                        {
                            Id = car.Id,
                            Make = car.Make,
                            Model = car.Model,
                            Color = car.Color,
                            StockCount = car.StockCount,
                            IsAvailable = car.IsAvailable
                        });
            
        }



        public async Task<PriorityCarDto> GetByIdAsync(int id)
        {
            return Maps.EntityCarToMapPriorityCarDto(  await _context.Cars.FindAsync(id));
        }

        public async Task<bool> SetAvailabilityAsync(int id, bool isAvailable)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return false;
            car.IsAvailable = isAvailable;
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateAsync(int id, PriorityCarDto car)
        {
            var updateCar = await _context.Cars.FindAsync(id);
            if (updateCar == null) return false;
            updateCar.Make = car.Make;
            updateCar.Color = car.Color;
            updateCar.Model = car.Model;
            updateCar.IsAvailable = car.IsAvailable;
            updateCar.StockCount = car.StockCount;
            _context.Cars.Update(updateCar);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(int id, int quantity)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return false;
            car.StockCount = quantity;
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return true;

        }

    }
}
