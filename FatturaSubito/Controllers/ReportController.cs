using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FatturaSubito.Controllers
{
    public class ReportController : Controller
    {
        DBContext db = new DBContext();

        protected Utente GetUtenteLoggato()
        {
            if (User.Identity.IsAuthenticated)
            {
                int utenteId;
                bool parseResult = int.TryParse(User.Identity.Name, out utenteId);
                if (parseResult)
                {
                    return db.Utente.Find(utenteId);
                }
            }
            return null;
        }

        // GET: Report
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new UserDashboardViewModel
            {
                TotaleFatturato = db.Documento
        .Where(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato)
        .SelectMany(d => d.RigheDocumento)
        .Sum(rd => (decimal?)rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)) ?? 0m,


                NumeroFattureVendita = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato),

                FattureInRitardo = db.Documento
        .Count(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && d.StatoValore == (int)StatoFattura.InRitardo && !d.Cestinato),
                
                FatturePagatoParzialmente = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && d.StatoValore == (int)StatoFattura.PagatoParzialmente && !d.Cestinato),

                FattureNonPagato = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && d.StatoValore == (int)StatoFattura.NonPagato && !d.Cestinato),

                FatturePagato = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaVendita && d.UtenteId == utenteLoggato.UtenteId && d.StatoValore == (int)StatoFattura.Pagato && !d.Cestinato),

                NumeroFattureAcquisto = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaAcquisto && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato),

                ValoreTotaleFattureAcquisto = db.Documento
                .Where(d => d.Tipo == TipoDocumento.FatturaAcquisto && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato)
                .SelectMany(d => d.RigheDocumento)
                .Sum(rd => (decimal?)rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)) ?? 0m,

                TotaleValorePreventivi = db.Documento
                .Where(d => d.Tipo == TipoDocumento.Preventivo && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato)
                .SelectMany(d => d.RigheDocumento)
                .Sum(rd => (decimal?)rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)) ?? 0m,

                NumeroTotalePreventivi = db.Documento
                .Count(d => d.Tipo == TipoDocumento.Preventivo && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato),

                NumeroBolleDiTrasporto = db.Documento
                .Count(d => d.Tipo == TipoDocumento.BollaDiTrasporto && d.UtenteId == utenteLoggato.UtenteId && !d.Cestinato)
            };

            
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Amministrazione()
        {
            var oggi = DateTime.Today;

            var viewModel = new AdminDashboardViewModel
            {
                NumeroFattureEmesse = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaVendita),

                TotaleValoreFatture = db.Documento
                .Where(d => d.Tipo == TipoDocumento.FatturaVendita)
                .SelectMany(d => d.RigheDocumento)
                .Sum(rd => (decimal?)rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)) ?? 0m,

                NumeroFattureAcquisto = db.Documento
                .Count(d => d.Tipo == TipoDocumento.FatturaAcquisto),

                NumeroPreventivi = db.Documento.Count(d => d.Tipo == TipoDocumento.Preventivo),

                TotaleValorePreventivi = db.Documento
                .Where(d => d.Tipo == TipoDocumento.Preventivo)
                .SelectMany(d => d.RigheDocumento)
                .Sum(rd => (decimal?)rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)) ?? 0m,

                NumeroBolleDiTrasporto = db.Documento
                .Count(d => d.Tipo == TipoDocumento.BollaDiTrasporto),

                TotaleNumeroUtenti = db.Utente.Count(),

                UtentiIscrittiOggi = db.Utente.Count(u => DbFunctions.TruncateTime(u.DataIscrizione) == oggi)

            };
            return View(viewModel);
        }
    }
}