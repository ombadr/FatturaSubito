using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FatturaSubito.Controllers
{
    public class FornitoriController : Controller
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
        // GET: Fornitori
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato != null)
            {
                var fornitori = db.Fornitore.Where(f => f.UtenteId == utenteLoggato.UtenteId).ToList();
                return View(fornitori);
            } else
            {
                return RedirectToAction("Login", "Auth");
            }
            
        }
        [Authorize]
        public ActionResult Aggiungi()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aggiungi([Bind(Include = "Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Indirizzo,Comune,Provincia,CAP")] Fornitore fornitore)
        {
            var utenteLoggato = GetUtenteLoggato();
            if(ModelState.IsValid)
            {
                fornitore.UtenteId = utenteLoggato.UtenteId;
                db.Fornitore.Add(fornitore);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fornitore);
        }
        [Authorize]
        public ActionResult Modifica(int id)
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato != null)
            {
                var fornitore = db.Fornitore.FirstOrDefault(f => f.FornitoreId == id && f.UtenteId == utenteLoggato.UtenteId);
                if(fornitore == null)
                {
                    return HttpNotFound();
                }
                return View(fornitore);
            }
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica([Bind(Include = "FornitoreId,Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Indirizzo,Comune,Provincia,CAP")] Fornitore fornitore)
        {
            var utenteLoggato = GetUtenteLoggato();
            if(ModelState.IsValid)
            {
                var fornitoreDb = db.Fornitore.FirstOrDefault(f => f.FornitoreId == fornitore.FornitoreId && f.UtenteId == utenteLoggato.UtenteId);
                if(fornitoreDb == null)
                {
                    return HttpNotFound();
                }

                fornitoreDb.Nome = fornitore.Nome;
                fornitoreDb.Cognome = fornitore.Cognome;
                fornitoreDb.NomeAzienda = fornitore.NomeAzienda;
                fornitoreDb.PartitaIVA = fornitore.PartitaIVA;
                fornitoreDb.CodiceFiscale = fornitore.CodiceFiscale;
                fornitoreDb.Email = fornitore.Email;
                fornitoreDb.Indirizzo = fornitore.Indirizzo;
                fornitoreDb.Comune = fornitore.Comune;
                fornitoreDb.Provincia = fornitore.Provincia;
                fornitoreDb.CAP = fornitore.CAP;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fornitore);
        }
    }
}