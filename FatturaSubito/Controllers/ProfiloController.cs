using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace FatturaSubito.Controllers
{
    public class ProfiloController : Controller
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
        // GET: Profilo
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato != null)
            {
                var profiloUtente = db.Utente.SingleOrDefault(u => u.UtenteId == utenteLoggato.UtenteId);
                return View(profiloUtente);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }

        }
        [Authorize]
        public ActionResult Modifica()
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato != null)
            {
                var viewModel = new ModificaProfiloViewModel
                {
                    UtenteId = utenteLoggato.UtenteId,
                    Nome = utenteLoggato.Nome,
                    Cognome = utenteLoggato.Cognome,
                    NomeAzienda = utenteLoggato.NomeAzienda,
                    PartitaIVA = utenteLoggato.PartitaIVA,
                    CodiceFiscale = utenteLoggato.CodiceFiscale,
                    Email = utenteLoggato.Email,
                    Indirizzo = utenteLoggato.Indirizzo,
                    Comune = utenteLoggato.Comune,
                    Provincia = utenteLoggato.Provincia,
                    CAP = utenteLoggato.CAP,
                    Logo = utenteLoggato.Logo,
                };
                return View(viewModel);
            }
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica([Bind(Include = "Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Indirizzo,Comune,Provincia,CAP")] ModificaProfiloViewModel model, HttpPostedFileBase Logo)
        {
            var utenteLoggato = GetUtenteLoggato();
            if (ModelState.IsValid)
            {
                if (Logo != null && Logo.ContentLength > 0)
                {
                    var directoryPath = Server.MapPath("~/Assets/uploads");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    if (!string.IsNullOrEmpty(utenteLoggato.Logo))
                    {
                        var oldFilePath = Server.MapPath(utenteLoggato.Logo);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var fileName = Path.GetFileName(Logo.FileName);
                    var path = Path.Combine(Server.MapPath("~/Assets/uploads"), fileName);
                    Logo.SaveAs(path);
                    utenteLoggato.Logo = $"~/Assets/uploads/{fileName}";
                }
         
                if (utenteLoggato != null)
                {
                    // utenteLoggato.UtenteId = model.UtenteId;
                    utenteLoggato.Nome = model.Nome;
                    utenteLoggato.Cognome = model.Cognome;
                    utenteLoggato.NomeAzienda = model.NomeAzienda;
                    utenteLoggato.PartitaIVA = model.PartitaIVA;
                    utenteLoggato.CodiceFiscale = model.CodiceFiscale;
                    utenteLoggato.Email = model.Email;
                    utenteLoggato.Indirizzo = model.Indirizzo;
                    utenteLoggato.Comune = model.Comune;
                    utenteLoggato.Provincia = model.Provincia;
                    utenteLoggato.CAP = model.CAP;

                    db.SaveChanges();

                    HttpCookie logoCookie = new HttpCookie("Logo", utenteLoggato.Logo);
                    if (Request.Cookies["Logo"] != null)
                    {
                        var existingCookie = Request.Cookies["Logo"];
                        existingCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(existingCookie);
                    }
                    logoCookie.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(logoCookie);

                    string fullName = $"{utenteLoggato.Nome} {utenteLoggato.Cognome}";
                    HttpCookie nomeUtenteCookie = new HttpCookie("NomeUtente", fullName);
                    if (Request.Cookies["NomeUtente"] != null)
                    {
                        var existingCookie = Request.Cookies["NomeUtente"];
                        existingCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(existingCookie);
                    }
                    nomeUtenteCookie.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(nomeUtenteCookie);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }
    }
}