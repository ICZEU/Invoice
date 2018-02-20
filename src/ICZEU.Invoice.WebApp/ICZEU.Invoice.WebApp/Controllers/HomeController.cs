using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ICZEU.Invoice.WebApp.Models;
using System;
using Microsoft.SharePoint.Client;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ICZEU.Invoice.WebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

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

        [HttpGet]
        public IActionResult Test()
        {
            var sb = new StringBuilder();

            string siteUrl = _config["SharePoint:SiteUrl"];
            Uri targetApplicationUri = new Uri(siteUrl);
            string targetRealm = TokenHelper.GetRealmFromTargetUrl(targetApplicationUri);
            var accessToken = TokenHelper.GetAppOnlyAccessToken
                (TokenHelper.SharePointPrincipal, targetApplicationUri.Authority, targetRealm).AccessToken;

            // we use the app-only access token to authenticate without the interaction of the user
            using (ClientContext context = TokenHelper.GetClientContextWithAccessToken(targetApplicationUri.ToString(), accessToken))
            {
                Web web = context.Web;

                context.Load(web);
                context.ExecuteQuery();
                sb.AppendLine(web.Title);

                List list = context.Web.Lists.GetByTitle("Rechnungseingang");
                CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
                ListItemCollection items = list.GetItems(query);
                context.Load(items);
                context.ExecuteQuery();
                foreach (ListItem item in items)
                {
                    sb.AppendLine((string) item["Title"]);
                }
            }

            return Content(sb.ToString());
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
