using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesApp.Validation
{
    public class AgeOfTheArtist: Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var date = DateTime.Parse(context.HttpContext.Request.Form["Birthday"]);
            var age = DateTime.Now.Year - date.Year;
            if (DateTime.Now.DayOfYear < date.DayOfYear) //на случай, если день рождения уже прошёл
                age++;
            if (age is < 7 or > 99)
            {
                context.Result = new BadRequestResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}