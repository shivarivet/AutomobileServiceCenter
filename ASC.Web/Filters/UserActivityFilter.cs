using ASC.Business.Interfaces;
using ASC.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Filters
{
    public class UserActivityFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();
            ILogDataOperations logDataOperations = context.HttpContext.RequestServices.GetService(typeof(ILogDataOperations)) as ILogDataOperations;
            if (context.HttpContext.User.GetCurrentUser() != null)
                await logDataOperations.CreateUserActivityAsync(context.HttpContext.User.GetCurrentUser().Email, context.HttpContext.Request.Path);
        }
    }
}
