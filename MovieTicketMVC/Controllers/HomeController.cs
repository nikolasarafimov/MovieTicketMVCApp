using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieTicketMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.Path == "/")
            {
                return RedirectPermanent("~/Home");
            }

            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}