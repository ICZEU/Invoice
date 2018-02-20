using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ICZEU.Invoice.WebApp.Models;
using System;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

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
            return View(PopulateViewModel(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(InvoiceFormModel model)
        {
            if (ModelState.IsValid)
            {
                UploadToSharePoint(model);
                return View("Success");
            }
            return View(PopulateViewModel(model));
        }

        private InvoiceFormModel PopulateViewModel(InvoiceFormModel model)
        {
            var options = GetCostCenterOptions();
            options.Sort();
            options.Insert(0, "");
            model.CostCenterItems = options.Select(
                opt => new SelectListItem { Text = opt, Value = opt }).ToList();
            return model;
        }

        private List<string> GetCostCenterOptions()
        {
            var options = new List<string>();
            using (ClientContext context = CreateClientContext())
            {
                Web web = context.Web;
                List list = context.Web.Lists.GetByTitle("Kostenstellen");
                CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
                ListItemCollection items = list.GetItems(query);
                context.Load(items);
                context.ExecuteQuery();
                foreach (ListItem item in items)
                {
                    options.Add((string) item["Title"]);
                }
            }
            return options;
        }

        private void UploadToSharePoint(InvoiceFormModel model)
        {
            using (ClientContext context = CreateClientContext())
            {
                Web web = context.Web;

                User author = web.EnsureUser(User.Identity.Name);
                context.Load(author);
                context.ExecuteQuery();

                List list = context.Web.Lists.GetByTitle("Rechnungseingang");
                ListItem item = list.AddItem(new ListItemCreationInformation());
                item["Title"] = model.Reason;
                item["Kostenstelle"] = model.CostCenter;
                item["Absender"] = new FieldUserValue { LookupId = author.Id };
                item.Update();
                context.ExecuteQuery();

                foreach(IFormFile uploadFile in model.Attachments)
                {
                    Attachment attachment = item.AttachmentFiles.Add(
                        new AttachmentCreationInformation
                        {
                            FileName = uploadFile.FileName,
                            ContentStream = uploadFile.OpenReadStream()
                        });
                    context.Load(attachment);
                    context.ExecuteQuery();
                }
            }
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Connect to SharePoint Online.
        /// </summary>
        private ClientContext CreateClientContext()
        {
            Uri targetApplicationUri = new Uri(_config["SharePoint:SiteUrl"]);

            string targetRealm = TokenHelper.GetRealmFromTargetUrl(targetApplicationUri);

            var accessToken = TokenHelper.GetAppOnlyAccessToken
                (TokenHelper.SharePointPrincipal, targetApplicationUri.Authority, targetRealm).AccessToken;

            // we use the app-only access token to authenticate without the interaction of the user
            return TokenHelper.GetClientContextWithAccessToken(targetApplicationUri.ToString(), accessToken);
        }
    }
}
