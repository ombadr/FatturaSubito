using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FatturaSubito.Controllers
{
    public class CestinoController : Controller
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
        // GET: Cestino
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato != null)
            {
                var documenti = db.Documento.Include("RigheDocumento.AliquotaIVA").Where(d => d.UtenteId == utenteLoggato.UtenteId && d.Cestinato == true).ToList();

                var documentiViewModel = documenti.Select(d => new DocumentoViewModel
                {
                    Id = d.DocumentoId,
                    Numero = d.Numero,
                    TipoDocumento = d.Tipo.ToString(),
                    Data = d.Data,
                    NomeAzienda = d.Cliente?.NomeAzienda ?? d.Fornitore?.NomeAzienda,
                    Email = d.Cliente?.Email ?? d.Fornitore?.Email,
                    TotaleDocumento = d.RigheDocumento.Sum(rd => rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100))
                }).ToList();

            return View(documentiViewModel);
            } else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RipristinaDocumento(int id)
        {

            var utenteLoggato = GetUtenteLoggato();
            var documento = db.Documento.Find(id);

            if(documento == null)
            {
                return HttpNotFound();
            }

            if(documento.UtenteId != utenteLoggato.UtenteId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            documento.Cestinato = false;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminaDocumento(int id)
        {
            var utenteLoggato = GetUtenteLoggato();
            var documento = db.Documento.Find(id);
            if(documento == null)
            {
                return HttpNotFound();
            }

            if (documento.UtenteId != utenteLoggato.UtenteId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.Documento.Remove(documento);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}