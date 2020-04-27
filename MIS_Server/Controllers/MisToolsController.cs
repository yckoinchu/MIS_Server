using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIS_Server.Controllers
{
    public class MisToolsController : Controller
    {
        // GET: MisTools
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult upload(HttpPostedFileBase file)
        {
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/KoData/uploads"), fileName);
                    file.SaveAs(path);
                }

            }

            return RedirectToAction("Index");
        }
    }
}