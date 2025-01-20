using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using CarGuideDDD.Core.DtObjects;

namespace API.ODataSettings
{
    public class DisableFilterAttribute : ActionFilterAttribute
    {
        private readonly string _propertyName;

        public DisableFilterAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Query.ContainsKey("$filter"))
            {
                var filterValue = context.HttpContext.Request.Query["$filter"].ToString();

                // Проверяем, есть ли фильтр по AddUserName
                if (filterValue.Contains(_propertyName))
                {
                    context.Result = new BadRequestObjectResult($"Фильтрация по полю '{_propertyName}' недоступна.");
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
