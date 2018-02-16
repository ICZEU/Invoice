using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ICZEU.Invoice.WebApp.Models;

namespace ICZEU.Invoice.WebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new InvoiceFormModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(InvoiceFormModel model)
        {
            if (ModelState.IsValid)
            {
                return View("Success");
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
