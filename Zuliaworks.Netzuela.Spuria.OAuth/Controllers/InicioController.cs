namespace Zuliaworks.Netzuela.Spuria.ServidorOAuth.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class InicioController : Controller
    {
        // GET: /Inicio/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AcercaDe()
        {
            return View();
        }
    }
}
