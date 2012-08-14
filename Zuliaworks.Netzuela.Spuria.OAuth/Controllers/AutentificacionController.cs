namespace Zuliaworks.Netzuela.Spuria.ServidorOAuth.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Zuliaworks.Netzuela.Spuria.ServidorOAuth;             // OAuth
    using Zuliaworks.Netzuela.Spuria.ServidorOAuth.Models;      // AutorizacionModel

    public class AutentificacionController : Controller
    {
        // GET: /Autentificacion/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Autentificacion/Autorizar/
        [HttpGet]
        public ActionResult Autorizar()
        {
            /*
            if (OAuth.PeticionDeAutorizacionPendiente == null)
            {
                return RedirectToAction("Index", "Inicio");
            }
            */
            var model = new AutorizacionModel
            {
                //AplicacionConsumidora = OAuthServiceProvider.PendingAuthorizationConsumer.Name,
                //PeticionInsegura = OAuth.PeticionDeAutorizacionPendiente.IsUnsafeRequest
                AplicacionConsumidora = "Valeria",
                PeticionInsegura = false
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Autorizar(AutorizacionModel autorizacion)
        {
            return View();
        }
    }
}
