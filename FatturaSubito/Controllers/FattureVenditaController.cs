using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Configuration;


namespace FatturaSubito.Controllers
{

    public class FattureVenditaController : Controller
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
        public string GetStatoFatturaDisplayString(StatoFattura stato)
        {
            switch (stato)
            {
                case StatoFattura.NonPagato:
                    return "Non pagato";
                case StatoFattura.Pagato:
                    return "Pagato";
                case StatoFattura.PagatoParzialmente:
                    return "Pagato parzialmente";
                case StatoFattura.InRitardo:
                    return "In ritardo";
                default:
                    return "Stato sconosciuto";
            }
        }

        // GET: FattureVendita
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato != null)
            {
                var documenti = db.Documento
                          .Include("RigheDocumento.AliquotaIVA")
                          .Where(d => d.UtenteId == utenteLoggato.UtenteId && d.Tipo == TipoDocumento.FatturaVendita && d.Cestinato == false)
                          .ToList();


                var documentiViewModel = documenti.Select(d => new DocumentoViewModel
                {
                    Id = d.DocumentoId,
                    Numero = d.Numero,
                    Data = d.Data,
                    NomeAzienda = d.Cliente.NomeAzienda,
                    Email = d.Cliente.Email,
                    TotaleDocumento = d.RigheDocumento.Sum(rd => rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)),
                    Stato = GetStatoFatturaDisplayString((StatoFattura)d.Stato)
                }).ToList();

                return View(documentiViewModel);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }
        [Authorize]
        public ActionResult Aggiungi()
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato != null)
            {
                var viewModel = new FatturaViewModel
                {
                    Documento = new Documento
                    {
                        Tipo = TipoDocumento.FatturaVendita,
                    },
                    Utente = utenteLoggato
                };
                ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomeAzienda");
                ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA, "AliquotaIVAId", "Descrizione");
                return View(viewModel);
            }
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aggiungi(FatturaViewModel model)
        {
            var utenteLoggato = GetUtenteLoggato();
            if (ModelState.IsValid)
            {
                model.Documento.UtenteId = utenteLoggato.UtenteId;
                db.Documento.Add(model.Documento);
                foreach (var riga in model.RigheDocumento)
                {
                    riga.DocumentoId = model.Documento.DocumentoId;
                    db.RigaDocumento.Add(riga);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomeAzienda", model.Documento.ClienteId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA, "AliquotaIVAId", "Descrizione");
            return View(model);
        }
        [Authorize]
        public ActionResult DettagliCliente(int id)
        {
            var utenteLoggato = GetUtenteLoggato();
            var cliente = db.Cliente.Find(id);

            if (cliente.UtenteId != utenteLoggato.UtenteId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            }
            if (cliente == null)
            {
                return HttpNotFound();
            }

            var clienteView = new
            {
                Nome = cliente.Nome,
                Cognome = cliente.Cognome,
                NomeAzienda = cliente.NomeAzienda,
                Indirizzo = cliente.Indirizzo,
                CAP = cliente.CAP,
                Comune = cliente.Comune,
                Provincia = cliente.Provincia,
                PartitaIVA = cliente.PartitaIVA,
                CodiceFiscale = cliente.CodiceFiscale,
            };

            return Json(clienteView, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CestinaDocumento(int id)
        {
            var uteneLoggato = GetUtenteLoggato();
            var documento = db.Documento.Find(id);

            if (documento.UtenteId != uteneLoggato.UtenteId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            }
            if (documento != null)
            {
                documento.Cestinato = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return HttpNotFound();
        }
        [Authorize]
        public ActionResult Modifica(int id)
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var documento = db.Documento
                .Include("RigheDocumento.AliquotaIVA")
                .SingleOrDefault(d => d.DocumentoId == id && d.UtenteId == utenteLoggato.UtenteId);

            if (documento == null)
            {
                return HttpNotFound();
            }

            var viewModel = new FatturaViewModel
            {
                Documento = documento,
                RigheDocumento = documento.RigheDocumento.ToList(),
                Utente = utenteLoggato
            };

            ViewBag.StatoFatturaSelectList = Enum.GetValues(typeof(StatoFattura))
                    .Cast<StatoFattura>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString().Replace("PagatoParzialmente", "Pagato parzialmente").Replace("InRitardo", "In ritardo").Replace("NonPagato", "Non pagato")
                    });
            ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomeAzienda", documento.ClienteId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA.ToList(), "AliquotaIVAId", "Descrizione");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica(FatturaViewModel model, string RigheEliminate)
        {
            System.Diagnostics.Debug.WriteLine($"RigheEliminate: {RigheEliminate}");

            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (ModelState.IsValid)

            {
                model.Utente = GetUtenteLoggato();
                var documentoDb = db.Documento
                    .Include("RigheDocumento")
                    .SingleOrDefault(d => d.DocumentoId == model.Documento.DocumentoId && d.UtenteId == utenteLoggato.UtenteId);

                if (documentoDb != null)
                {
                    documentoDb.Data = model.Documento.Data;
                    documentoDb.Stato = model.StatoSelezionato;
                    documentoDb.Numero = model.Documento.Numero;
                    documentoDb.TerminiCondizioni = model.Documento.TerminiCondizioni;

                    if (model.RigheDocumento == null)
                    {
                        model.RigheDocumento = new List<RigaDocumento>();
                    }

                    var existingRigheIds = model.RigheDocumento.Select(r => r.RigaDocumentoId).ToList();

                    var righeToRemove = documentoDb.RigheDocumento.Where(r => !existingRigheIds.Contains(r.RigaDocumentoId)).ToList();
                    foreach (var rigaToRemove in righeToRemove)
                    {
                        db.RigaDocumento.Remove(rigaToRemove);
                    }

                    foreach (var rigaViewModel in model.RigheDocumento)
                    {
                        if (rigaViewModel.RigaDocumentoId != 0)
                        {


                            var rigaDb = documentoDb.RigheDocumento.SingleOrDefault(r => r.RigaDocumentoId == rigaViewModel.RigaDocumentoId);
                            if (rigaDb != null)
                            {
                                rigaDb.Descrizione = rigaViewModel.Descrizione;
                                rigaDb.Quantita = rigaViewModel.Quantita;
                                rigaDb.PrezzoUnitario = rigaViewModel.PrezzoUnitario;
                                rigaDb.AliquotaIVAId = rigaViewModel.AliquotaIVAId;
                            }
                        }
                        else
                        {
                            documentoDb.RigheDocumento.Add(new RigaDocumento
                            {
                                Descrizione = rigaViewModel.Descrizione,
                                Quantita = rigaViewModel.Quantita,
                                PrezzoUnitario = rigaViewModel.PrezzoUnitario,
                                AliquotaIVAId = rigaViewModel.AliquotaIVAId,
                                DocumentoId = documentoDb.DocumentoId
                            });
                        }
                    }

                    if (!String.IsNullOrEmpty(RigheEliminate))
                    {
                        var idRigheEliminate = RigheEliminate.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(id =>
                            {
                                bool isParsed = int.TryParse(id, out int parsedId);
                                if (isParsed)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Eliminazione riga con ID: {parsedId}");
                                }
                                return parsedId;
                            })
                            .Where(id => id != 0)
                            .ToList();

                        foreach (var id in idRigheEliminate)
                        {
                            var rigaDaRimuovere = db.RigaDocumento.Find(id);
                            if (rigaDaRimuovere != null)
                            {
                                db.RigaDocumento.Remove(rigaDaRimuovere);
                                System.Diagnostics.Debug.WriteLine($"Rimuovendo riga con ID: {id}");
                            }
                        }
                    }

                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("Modifiche salvate correttamente.");

                    return RedirectToAction("Index");
                }
            }
            ViewBag.StatoFatturaSelectList = Enum.GetValues(typeof(StatoFattura))
                    .Cast<StatoFattura>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString().Replace("PagatoParzialmente", "Pagato parzialmente").Replace("InRitardo", "In ritardo").Replace("NonPagato", "Non pagato")
                    });
            ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomeAzienda", model.Documento.ClienteId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA.ToList(), "AliquotaIVAId", "Descrizione");
            return View(model);
        }


        public ActionResult ScaricaPdf(int id)
        {
            var utenteLoggato = GetUtenteLoggato();

            if (utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var documento = db.Documento
                .Include("RigheDocumento.AliquotaIVA")
                .SingleOrDefault(d => d.DocumentoId == id && d.UtenteId == utenteLoggato.UtenteId);

            if (documento == null)
            {
                return HttpNotFound();
            }

            var viewModel = new FatturaViewModel
            {
                Documento = documento,
                RigheDocumento = documento.RigheDocumento.ToList(),
                Utente = utenteLoggato
            };

            return new Rotativa.ViewAsPdf("FatturaVenditaPdf", viewModel)
            {
                FileName = $"Fattura_Vendita_{documento.Numero}.pdf",
                CustomSwitches = "--no-stop-slow-scripts --javascript-delay 3000"
            };
        }

        private void InviaEmailConPdf(int id, string emailDestinatario, string emailMittente)
        {
            var documento = db.Documento.Include("RigheDocumento.AliquotaIVA").SingleOrDefault(d => d.DocumentoId == id);

            if (documento == null) return;

            var viewModel = new FatturaViewModel
            {
                Documento = documento,
                RigheDocumento = documento.RigheDocumento.ToList(),
                Utente = GetUtenteLoggato()
            };

            var pdf = new Rotativa.ViewAsPdf("FatturaVenditaPdf", viewModel)
            {
                FileName = "Fattura.pdf"
            };

            var binarioPdf = pdf.BuildFile(ControllerContext);

            using (var messaggio = new MailMessage())
            {
                messaggio.From = new MailAddress(emailMittente);
                messaggio.To.Add(new MailAddress(emailDestinatario));
                messaggio.CC.Add(new MailAddress(emailMittente));
                messaggio.Subject = "La tua fattura";
                messaggio.Body = "Ecco la fattura richiesta.";
                messaggio.Attachments.Add(new Attachment(new MemoryStream(binarioPdf), "Fattura.pdf"));

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = "smtp.mailgun.org";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential("", "");
                    smtpClient.Send(messaggio);
                }
            }
        }

        public ActionResult InviaEmail(int id)
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato == null) return RedirectToAction("Login", "Auth");

            var documento = db.Documento.Include("Cliente").SingleOrDefault(d => d.DocumentoId == id);
            if (documento == null) return HttpNotFound();

            try
            {
                var emailDestinatario = documento.Cliente.Email;
                var emailMittente = utenteLoggato.Email; // receive copy of document

                InviaEmailConPdf(id, emailDestinatario, emailMittente);
                TempData["SuccessMessage"] = "Email inviata con successo!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Si è verificato un errore nell'invio dell'email.";
            }
            return RedirectToAction("Index");
        }

    }
}
