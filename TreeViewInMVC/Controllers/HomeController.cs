﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TreeViewInMVC.Models;

namespace TreeViewInMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult TreeView()
        {
            var db = new ApplicationDbContext();
            return View(db.TreeModel.Where(x => !x.ParentId.HasValue).ToList());
        }

        [HttpPost]
        public ActionResult TreeView(List<TreeModel> model)
        {
            return View(model);
        }

    }
}