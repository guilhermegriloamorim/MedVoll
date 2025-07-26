using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedVoll.Web.Filters
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
#if !DEBUG
            if (context.Exception is KeyNotFoundException || context.Exception is InvalidOperationException)
            {
                context.Result = new RedirectToActionResult("404", "Erro", null);
                context.ExceptionHandled = true;
            }
            else
            {
                context.Result = new RedirectToActionResult("500", "Erro", null);
                context.ExceptionHandled = true;
            }
#endif
        }
    }
}
