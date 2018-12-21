using MyProjectRestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyProjectRestApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //ApplicationDbContext db = new ApplicationDbContext();
            //ApplicationUser applicationUser = new ApplicationUser() { UserName = "Jeckson" };
            //db.Users.Add(applicationUser);
            //db.SaveChanges();

            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
