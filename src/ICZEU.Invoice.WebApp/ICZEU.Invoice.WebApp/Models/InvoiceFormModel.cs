using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICZEU.Invoice.WebApp.Models
{
    public class InvoiceFormModel
    {
        public const string StatusNotPaid = "notpaid";
        public const string StatusPaid = "paid";

        public IList<SelectListItem> CostCenterItems { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Bitte Auswahl treffen!")]
        [Display(Name = "Kostenstelle / Arbeitsbereich")]
        public string CostCenter { get; set; }

        [Required(ErrorMessage = "Bitte Verwendungszweck angeben!")]
        [Display(Name = "Verwendungszweck")]
        public string Reason { get; set; }

        public SelectListItem[] StatusList { get; set; } = new[]
{
            new SelectListItem { Value = StatusNotPaid, Text = "zu bezahlen" },
            new SelectListItem { Value = StatusPaid, Text = "bereits ausgelegt" }
        };

        [Display(Name = "Das Geldbetrag ist")]
        public string Status { get; set; }

        [Display(Name = "IBAN (optional)")]
        public string IBAN { get; set; }

        [Display(Name = "Kopie der Antwort senden an (optional)")]
        [EmailAddress(ErrorMessage = "Bitte gültige E-Mail-Adresse eingeben!")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Bitte lade mindestens eine Datei hoch!")]
        [Display(Name = "Rechnung als PDF oder Bild")]
        public List<IFormFile> Attachments { get; set; }
    }
}
