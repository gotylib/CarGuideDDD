using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Domain.Entities;
using DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (car != null)
            {
                _context.Cars.Remove(Maps.MapPriorityCarDtoToEntityCar(car));
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<PriorityCarDto>> GetAllAsync(bool priority)
        {
            if (priority)
            {
                return (await _context.Cars.ToListAsync()).Select(Maps.EntityCarToMapPriorityCarDto);
            }
            else
            {
                return ((await _context.Cars.Where(c => c.IsAvailable == true).ToListAsync()).Select(Maps.EntityCarToMapPriorityCarDto));
            }
        }

        public async Task<PriorityCarDto?> GetByIdAsync(int id)
        {
            return ( await _context.Cars.FindAsync(id)).;
        }

        public async Task<bool> SetAvailabilityAsync(int id, bool isAvailable)
        {
            var car = _context.Cars.Find(id);
            if (car != null)
            {
                car.IsAvailable = isAvailable;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(int id, PriorityCarDto car)
        {
            var updateCar = _context.Cars.Find(id);
            if (updateCar != null)
            {
                updateCar.Make = car.Make;
                updateCar.Color = car.Color;
                updateCar.Model = car.Model;
                updateCar.IsAvailable = car.IsAvailable;
                updateCar.StockCount = car.StockCount;
                _context.Cars.Update(updateCar);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> UpdateQuantityAsync(int id, int quantity)
        {
            var car = _context.Cars.Find(id);
            if (car != null)
            {
                car.StockCount = quantity;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
