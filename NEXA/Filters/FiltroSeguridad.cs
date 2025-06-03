using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace NEXA.Filters
{
    public class FiltroSeguridadAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                // Redirigir al login si no hay sesión
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    {"controller", "Acceso"},
                    {"action", "InicioSesion"}
                });
            }
            base.OnActionExecuting(context);
        }
    }
}
