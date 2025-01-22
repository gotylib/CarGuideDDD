using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using CarGuideDDD.Infrastructure.Services.Сhecks;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Infrastructure.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _repository;
        public BasketService(IBasketRepository repository)
        {
            _repository = repository;
        }
        public async Task<ServiceResult<VoidDto,Exception,AddCarToBasketDto>> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name)
        {
            if (RolesCheck.IsManager(roles))
            {
                return ServiceResult<VoidDto,Exception,AddCarToBasketDto>.BadRequest();
            }
            if (!RolesCheck.IsAdmin(roles) && addCarToBasketDto.UserName != name)
            {
                ServiceResult<VoidDto,Exception,AddCarToBasketDto>.BadRequest();
            }
            if (RolesCheck.IsUser(roles))
            {
                if (await _repository.AddCarToBasket(addCarToBasketDto))
                {
                    return ServiceResult<VoidDto,Exception,AddCarToBasketDto>.SimpleResult(addCarToBasketDto);
                }
            }

            return  ServiceResult<VoidDto,Exception,AddCarToBasketDto>.ServerError();
        }

        public async Task<ServiceResult<VoidDto,Exception, DeleteCarFromBasketDto>> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name)
        {
            if (RolesCheck.IsManager(roles))
            {
                ServiceResult<VoidDto, Exception, DeleteCarFromBasketDto>.BadRequest();
            }
            if(!RolesCheck.IsAdmin(roles) && deleteCarFromBasketDto.UserName != name)
            {
                ServiceResult<VoidDto, Exception, DeleteCarFromBasketDto>.BadRequest();
            }
            if (RolesCheck.IsUser(roles))
            {
                if (await _repository.DeleteCar(deleteCarFromBasketDto))
                {
                    return ServiceResult<VoidDto, Exception, DeleteCarFromBasketDto>.Ok();
                }
            }

            return  ServiceResult<VoidDto,Exception,DeleteCarFromBasketDto>.ServerError();
        }

        public async Task<ServiceResult<EntityUser,Exception,EntityBasket>> GetCarFromBasker(List<string> roles, string name)
        {
            if (RolesCheck.IsManager(roles))
            {
                return ServiceResult<EntityUser, Exception, EntityBasket>.BadRequest();
            }
            if (RolesCheck.IsAdmin(roles))
            {
                var resultAdmin = await _repository.GetAllBaskets();

                return resultAdmin.IsSuccessful
                    ? ServiceResult<EntityUser, Exception, EntityBasket>.IEnumerableResult(resultAdmin.Value)
                    : ServiceResult<EntityUser, Exception, EntityBasket>.ErrorResult(resultAdmin.Error);
            }
            if (RolesCheck.IsUser(roles))
            {
                var result = await _repository.GetBasket(name);

                return result.IsSuccessful
                    ? ServiceResult<EntityUser, Exception, EntityBasket>.SimpleResult(result.Value)
                    : ServiceResult<EntityUser, Exception, EntityBasket>.ErrorResult(result.Error);
            }
            return ServiceResult<EntityUser, Exception, EntityBasket>.ServerError();
        }

        public async Task<ServiceResult<VoidDto,Exception, UpdateColorDto>> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name)
        {

            if (RolesCheck.IsManager(roles))
            {
                return ServiceResult<VoidDto, Exception, UpdateColorDto>.BadRequest();
            }
            if (!RolesCheck.IsAdmin(roles) || updateColorDto.UserName != name)
            {
                return ServiceResult<VoidDto, Exception, UpdateColorDto>.BadRequest();
            }
            if (RolesCheck.IsUser(roles))
            {
                if (await _repository.UpdateCarColor(updateColorDto))
                {
                    return ServiceResult<VoidDto, Exception, UpdateColorDto>.SimpleResult(updateColorDto);
                }
                
            }
            return ServiceResult<VoidDto, Exception, UpdateColorDto>.ServerError();
        }
    }
}
