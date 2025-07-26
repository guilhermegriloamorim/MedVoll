using MedVoll.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedVoll.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        public BaseController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!CheckSessionSecurity())
            {
                context.Result = new RedirectResult("./Login");
            }

            ViewData["Especialidades"] = GetEspecialidades();
            ViewData["VollMedCard"] = HttpContext.Session.GetString("VollMedCard");
            base.OnActionExecuting(context);
        }

        private List<Especialidade> GetEspecialidades()
        {
            var especialidades = (Especialidade[])Enum.GetValues(typeof(Especialidade));
            return especialidades.ToList();
        }

        private bool CheckSessionSecurity()
        {
            var currentIp = HttpContext.Connection.RemoteIpAddress.ToString();
            var sessionIp = HttpContext.Session.GetString("IpAddress");

            if (sessionIp != null && sessionIp != currentIp)
            {
                HttpContext.Session.Clear();
                _signInManager.SignOutAsync();
                return false;
            }

            HttpContext.Session.SetString("IpAddress", currentIp);
            return true;
        }

    }
}
