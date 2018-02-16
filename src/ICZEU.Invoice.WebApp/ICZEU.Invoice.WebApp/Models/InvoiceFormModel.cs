using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICZEU.Invoice.WebApp.Models
{
    public class InvoiceFormModel
    {
        public InvoiceFormModel()
        {
            CostCenterItems.Add(new SelectListItem { Text = "KidsClub", Value = "KidsClub" });
            CostCenterItems.Add(new SelectListItem { Text = "Renovierung Thränstr. 23/2", Value = "Renovierung Thränstr. 23/2" });
            CostCenterItems.Add(new SelectListItem { Text = "Sonstiger", Value = "" });
        }

        public IList<SelectListItem> CostCenterItems { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Bitte Auswahl treffen!")]
        [Display(Name = "Kostenstelle / Arbeitsbereich")]
        public string CostCenter { get; set; }

        [Required(ErrorMessage = "Bitte Verwendungszweck angeben!")]
        [Display(Name = "Verwendungszweck")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Bitte lade mindestens eine Datei hoch!")]
        [Display(Name = "Rechnung als PDF oder Bild")]
        public List<IFormFile> Attachments { get; set; }
    }
}
