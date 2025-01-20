
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarGuideDDD.Infrastructure.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<BasketRepository> _logger;
        public BasketRepository(ApplicationDbContext context, ILogger<BasketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<bool> AddCarToBasket(AddCarToBasketDto addCarToBasketDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Basket)
                    .ThenInclude(b => b.Cars)
                    .FirstOrDefaultAsync(u => u.UserName == addCarToBasketDto.Username);

                if (user == null) { return false; }

                var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == addCarToBasketDto.CatId);

                if (car == null) { return false; }

                if (user.Basket == null)
                {
                    user.Basket = new EntityBasket
                    {
                        Cars = new List<EntityCar> { car }
                    };

                }
                else
                {
                    user.Basket.Cars.Add(car);
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error add to basket", ex);
                return false;
            }
        }

        public async Task<bool> DeleteCar(DeleteCarFromBasketDto deleteCarFromBasketDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Basket)
                    .ThenInclude(b => b.Cars)
                    .FirstOrDefaultAsync(u => u.UserName == deleteCarFromBasketDto.UserName);

                if (user == null) { return false; }
                 


                if (user.Basket == null)
                {
                    user.Basket = new EntityBasket();

                    return true;
                }
                else
                {
                    var car = user.Basket.Cars.FirstOrDefault(c => c.Id == deleteCarFromBasketDto.CarId);

                    if (car == null) { return false; }

                    user.Basket.Cars.Remove(car);

                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error delete to basket", ex);
                return false;
            }
        }

        public async Task<ResultModel<IEnumerable<EntityUser>, Exception>> GetAllBaskets()
        {
            try
            {
                var usersWithBaskets = await _context.Users
                    .Include(u => u.Basket)
                    .ThenInclude(b => b.Cars)
                    .ToListAsync();

                return ResultModel<IEnumerable<EntityUser>, Exception>.CreateSuccessfulResult(usersWithBaskets);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting all baskets", ex);
                return ResultModel<IEnumerable<EntityUser>, Exception>.CreateFailedResult(ex);
            }
        }



        public async Task<ResultModel<EntityBasket, Exception>> GetBasket(string username)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Basket)
                    .ThenInclude(b => b.Cars)
                    .FirstOrDefaultAsync(u => u.UserName == username);

                if (user == null || user.Basket == null)
                {
                    return ResultModel<EntityBasket, Exception>.CreateFailedResult(new Exception("Basket not found"));
                }

                return ResultModel<EntityBasket, Exception>.CreateSuccessfulResult(user.Basket);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting basket", ex);
                return ResultModel<EntityBasket, Exception>.CreateFailedResult(ex);
            }
        }


        public async Task<bool> UpdateCarColor(UpdateColorDto updateColorDto)
        {
            try
            {
                // Найти пользователя по имени
                var user = await _context.Users
                    .Include(u => u.Basket)
                    .ThenInclude(b => b.Cars)
                    .FirstOrDefaultAsync(u => u.UserName == updateColorDto.UserName);

                if (user == null || user.Basket == null) { return false; }


                // Найти автомобиль в корзине пользователя
                var car = user.Basket.Cars.FirstOrDefault(c => c.Id == updateColorDto.CarId);

                if (car == null) { return false; }


                // Найти цвет по идентификатору
                var color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == updateColorDto.ColorId);

                if (color == null) { return false; }

                // Переназначить объект цвета
                car.Color = color;

                // Сохранить изменения в базе данных
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating car color", ex);
                return false;
            }
        }


    }
}
