using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;


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
            var color = _context.Colors.FirstOrDefault(c => c.Color == car.Color);
            if (color == null)
            {
                var addCar = Maps.MapPriorityCarDtoToEntityCar(car);
                addCar.Color = new EntityColor() { Color = car.Color};
                await _context.Cars.AddAsync(addCar);
            }
            else
            {
                var addCar = Maps.MapPriorityCarDtoToEntityCar(car);
                addCar.Color = new EntityColor() { Color = color.Color, Id = color.Id };
                await _context.Cars.AddAsync(addCar);
            }
            if (car.NameOfPhoto.IsNullOrEmpty())
            {
                await _context.CarWithoutPhotos.AddAsync(Maps.MapPriorityCarDtoToEntityCarWithoutPhoto(car));
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddCarPhotoAsync(CarPhotoDto carPhotoDto, string guid)
        {
            // Найти существующую запись car
            var car = _context.Cars.FirstOrDefault(car =>
                car.Make == carPhotoDto.Make &&
                car.Model == carPhotoDto.Model &&
                car.Color.Color == carPhotoDto.Color);

            // Найти существующую запись carWithoutPhoto
            var carWithoutPhoto = _context.CarWithoutPhotos.FirstOrDefault(car =>
                car.Make == carPhotoDto.Make &&
                car.Model == carPhotoDto.Model &&
                car.Color == carPhotoDto.Color);

            // Если car не найден, вернуть false
            if (car == null)
            {
                return false;
            }

            // Обновить запись car
            car.NameOfPhoto = guid;
            _context.Cars.Update(car);

            // Если carWithoutPhoto найден, удалить его
            if (carWithoutPhoto != null)
            {
                _context.CarWithoutPhotos.Remove(carWithoutPhoto);
            }

            // Сохранить изменения в базе данных
            _context.SaveChanges();

            return true;

        }

        public async Task<bool> DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null) { return false; }
            _context.Cars.Remove(car);
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
                    Color = car.Color.Color,
                    NameOfPhoto = car.NameOfPhoto,
                    AddUserName = car.AddUserName,
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
                            Color = car.Color.Color,
                            NameOfPhoto = car.NameOfPhoto,
                            AddUserName = car.AddUserName,
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
            updateCar.Color = _context.Colors.FirstOrDefault(c => c.Color == car.Color) ?? null;
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
