using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PlatformEducationWorkers.Attributes
{
    public class UserExistsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            // Перевіряємо, чи присутні необхідні значення в сесії
            var userId = httpContext.Session.GetString("UserId");
            var userRole = httpContext.Session.GetString("UserRole");
            var enterpriseId = httpContext.Session.GetString("EnterpriseId");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(enterpriseId))
            {
                // Якщо хоч одне значення відсутнє, перенаправляємо на сторінку логіну
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
