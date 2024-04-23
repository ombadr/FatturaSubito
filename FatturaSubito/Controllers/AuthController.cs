using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FatturaSubito.Models;
using System.IO;


namespace FatturaSubito.Controllers
{
    public class AuthController : Controller
    {
        DBContext db = new DBContext();

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Report");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "Email,Password")] Utente user)
        {
            bool keepUserLogged = Request.Form["keepLogged"] == "true";
            // bool keepUserLogged = keepLogged.HasValue && keepLogged.Value;
            var loggedUser = db.Utente.FirstOrDefault(u => u.Email == user.Email && u.Attivo == true);

            if (loggedUser != null)
            {
                bool validPassword = BCrypt.Net.BCrypt.Verify(user.Password, loggedUser.Password);
                if (validPassword)
                {
                    FormsAuthentication.SetAuthCookie(loggedUser.UtenteId.ToString(), keepUserLogged);


                    if (keepUserLogged)
                    {
                        HttpCookie nomeUtente = new HttpCookie("NomeUtente", loggedUser.Nome + " " + loggedUser.Cognome)
                        {
                            Expires = DateTime.Now.AddDays(1)
                        };
                        Response.Cookies.Add(nomeUtente);

                        HttpCookie logoCookie = new HttpCookie("Logo", loggedUser.Logo)
                        {
                            Expires = DateTime.Now.AddDays(1)
                        };
                        Response.Cookies.Add(logoCookie);
                    } else
                    {
                        HttpCookie nomeUtente = new HttpCookie("NomeUtente", loggedUser.Nome + " " + loggedUser.Cognome)
                        {
                            Expires = DateTime.Now.AddMinutes(15)
                        };
                        Response.Cookies.Add(nomeUtente);

                        HttpCookie logoCookie = new HttpCookie("Logo", loggedUser.Logo)
                        {
                            Expires = DateTime.Now.AddMinutes(15)
                        };
                        Response.Cookies.Add(logoCookie);
                    }

                    return RedirectToAction("Index", "Report");
                }
            } else
            {
                return View();
            }

            return View();
        }

        public ActionResult Registrazione()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Report");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrazione([Bind(Include = "Nome,Cognome,NomeAzienda,PartitaIVA,CodiceFiscale,Email,Password,ConfermaPassword,Indirizzo,Comune,Provincia,CAP")] RegistrazioneViewModel model, HttpPostedFileBase Logo)
        {
            if (ModelState.IsValid)
            {
                var directoryPath = Server.MapPath("~/Assets/uploads");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Path.GetFileName(Logo.FileName);
                var path = Path.Combine(Server.MapPath("~/Assets/uploads"), fileName);
                Logo.SaveAs(path);
                var existingUser = db.Utente.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser == null)
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    var newUser = new Utente
                    {
                        Nome = model.Nome.Trim(),
                        Cognome = model.Cognome.Trim(),
                        NomeAzienda = model.NomeAzienda.Trim(),
                        PartitaIVA = model.PartitaIVA.Trim(),
                        CodiceFiscale = model.CodiceFiscale.Trim(),
                        Email = model.Email.Trim(),
                        Password = hashedPassword,
                        Indirizzo = model.Indirizzo.Trim(),
                        Comune = model.Comune.Trim(),
                        Provincia = model.Provincia.Trim(),
                        CAP = model.CAP.Trim(),
                        Logo = $"~/Assets/uploads/{fileName}",
                        Attivo = true,
                        Ruolo = "User",
                        DataIscrizione = DateTime.Now
                    };

                    db.Utente.Add(newUser);
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(newUser.UtenteId.ToString(), false);
                    HttpCookie nomeUtente = new HttpCookie("NomeUtente", newUser.Nome + " " + newUser.Cognome)
                    {
                        Expires = DateTime.Now.AddMinutes(15)
                    };
                    Response.Cookies.Add(nomeUtente);
                    HttpCookie logoCookie = new HttpCookie("Logo", newUser.Logo)
                    {
                        Expires = DateTime.Now.AddMinutes(15)
                    };
                    Response.Cookies.Add(logoCookie);
                    return RedirectToAction("Index", "Report");
                }
                else
                {
                    ModelState.AddModelError("", "Un account con questo indirizzo email esiste già.");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}