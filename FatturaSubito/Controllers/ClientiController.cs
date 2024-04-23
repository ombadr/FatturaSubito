using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FatturaSubito.Controllers
{
    public class ClientiController : Controller
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

        // GET: Clienti
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();

            if(utenteLoggato != null)
            {
                var clienti = db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId).ToList();
                return View(clienti); 
            }
            else
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
        public ActionResult Aggiungi([Bind(Include = "Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Indirizzo,Comune,Provincia,CAP")] Cliente cliente)
        {
            var utenteLoggato = GetUtenteLoggato();

            if (ModelState.IsValid)
            {
                cliente.UtenteId = utenteLoggato.UtenteId;
                db.Cliente.Add(cliente);
                db.SaveChanges();
                return RedirectToAction("Index");   
            }
            return View(cliente);
        }
        [Authorize]
        public ActionResult Modifica(int id)
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato != null)
            {
                var cliente = db.Cliente.FirstOrDefault(c => c.ClienteId == id && c.UtenteId == utenteLoggato.UtenteId);
                if (cliente == null)
                {
                    return HttpNotFound();
                }

                return View(cliente);
            }

            return RedirectToAction("Login", "Auth");
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica([Bind(Include = "ClienteId,Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Indirizzo,Comune,Provincia,CAP")] Cliente cliente)
        {
            var utenteLoggato = GetUtenteLoggato();

            if (ModelState.IsValid)
            {
                var clienteDb = db.Cliente.FirstOrDefault(c => c.ClienteId == cliente.ClienteId);
                if(clienteDb == null)
                {
                    return HttpNotFound();
                }

                if(clienteDb.UtenteId != utenteLoggato.UtenteId)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                }

                clienteDb.Nome = cliente.Nome;
                clienteDb.Cognome = cliente.Cognome;
                clienteDb.NomeAzienda = cliente.NomeAzienda;
                clienteDb.PartitaIVA = cliente.PartitaIVA;
                clienteDb.CodiceFiscale = cliente.CodiceFiscale;
                clienteDb.Email = cliente.Email;
                clienteDb.Indirizzo = cliente.Indirizzo;
                clienteDb.Comune = cliente.Comune;
                clienteDb.Provincia = cliente.Provincia;
                clienteDb.CAP = cliente.CAP;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cliente);
        }
    }
}