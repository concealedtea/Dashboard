using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevenueChart.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About this Page";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Info";

            return View();
        }

        public ActionResult Basic()
        {
            return View();
        }
    }
}