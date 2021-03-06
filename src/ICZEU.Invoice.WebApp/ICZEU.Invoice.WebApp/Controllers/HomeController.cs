﻿using ICZEU.Invoice.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            IList<SelectListItem> options = GetCostCenterOptions()
                .Select(opt => new SelectListItem { Text = opt, Value = opt })
                .OrderBy(x => x.Text)
                .ToList();
            options.Insert(0, new SelectListItem
            {
                Value = "Neuer Arbeitsbereich",
                Text = "Neuer Arbeitsbereich. Bitte im Verwendungszweck den Arbeitsbereich mit angeben!"
            });
            model.CostCenterItems = options;
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
                item["Ausgelegt"] = model.Status == InvoiceFormModel.StatusPaid;
                item["IBAN"] = model.IBAN;
                item["CC"] = model.EmailAddress;
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
