using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FatturaSubito.Controllers
{
    public class AmministrazioneController : Controller
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
        [Authorize(Roles = "Admin")]
        public ActionResult GestioneUtenti()
        {
            var utenti = db.Utente.ToList();
            return View(utenti);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AliquoteIVA()
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato != null)
            {
                var aliquoteIVA = db.AliquotaIVA.ToList();
                return View(aliquoteIVA);
            } else
            {
                return RedirectToAction("Login", "Auth");
            }
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ModificaAliquotaIVA(int id)
        {
            var utenteLoggato = GetUtenteLoggato();

            if(utenteLoggato != null)
            {
                var aliquotaIVA = db.AliquotaIVA.FirstOrDefault(a => a.AliquotaIVAId == id);
                if(aliquotaIVA == null)
                {
                    return HttpNotFound();
                }

                return View(aliquotaIVA);
            }
            return RedirectToAction("Login", "Auth");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AggiungiAliquotaIVA()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AggiungiAliquotaIVA([Bind(Include = "Descrizione,ValorePercentuale")]AliquotaIVA aliquotaIVA)
        {
            if(ModelState.IsValid)
            {
                db.AliquotaIVA.Add(aliquotaIVA);
                db.SaveChanges();
                return RedirectToAction("AliquoteIVA");
            }
            return View(aliquotaIVA);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModificaAliquotaIVA(int id, [Bind(Include = "Descrizione,ValorePercentuale")] AliquotaIVA model)
        {
            var aliquotaIVADb = db.AliquotaIVA.FirstOrDefault(a => a.AliquotaIVAId == id);
            if (aliquotaIVADb == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                aliquotaIVADb.Descrizione = model.Descrizione;
                aliquotaIVADb.ValorePercentuale = model.ValorePercentuale;
                db.SaveChanges();
                return RedirectToAction("AliquoteIVA");
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BloccaUtente(int id)
        {
            var utente = db.Utente.Find(id);
            if (utente != null)
            {
                utente.Attivo = false;
                db.SaveChanges();
                return RedirectToAction("GestioneUtenti");
            }
            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SbloccaUtente(int id)
        {
            var utente = db.Utente.Find(id);
            if (utente != null)
            {
                utente.Attivo = true;
                db.SaveChanges();
                return RedirectToAction("GestioneUtenti");
            }
            return HttpNotFound();
        }
    }
}