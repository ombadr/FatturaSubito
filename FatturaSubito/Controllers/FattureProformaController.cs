using FatturaSubito.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor;

namespace FatturaSubito.Controllers
{
    public class FattureProformaController : Controller
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

        public string GetStatoFatturaDisplayString(StatoProformaPreventivo stato)
        {
            switch (stato)
            {
                case StatoProformaPreventivo.InAttesaDiApprovazione:
                    return "In attesa di approvazione";
                case StatoProformaPreventivo.Approvato:
                    return "Approvato";
                case StatoProformaPreventivo.Rifiutato:
                    return "Rifiutato";
                default:
                    return "Stato sconosciuto";
            }
        }
        // GET: FattureProforma
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato != null)
            {
                var documenti = db.Documento
                    .Include("RigheDocumento.AliquotaIVA")
                    .Where(d => d.UtenteId == utenteLoggato.UtenteId && d.Tipo == TipoDocumento.Proforma && d.Cestinato == false)
                    .ToList();

                var documentiViewModel = documenti.Select(d => new DocumentoViewModel
                {
                    Id = d.DocumentoId,
                    Numero = d.Numero,
                    Data = d.Data,
                    NomeAzienda = d.Cliente.NomeAzienda,
                    Email = d.Cliente.Email,
                    TotaleDocumento = d.RigheDocumento.Sum(rd => rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)),
                    Stato = GetStatoFatturaDisplayString((StatoProformaPreventivo)d.Stato)
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
                        Tipo = TipoDocumento.Proforma
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

            ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomaAzienda", model.Documento.ClienteId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA, "AliquotaIVAId", "Descrizione");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CestinaDocumento(int id)
        {
            var utenteLoggato = GetUtenteLoggato();

            var documento = db.Documento.Find(id);

            if(documento.UtenteId != utenteLoggato.UtenteId)
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
            ViewBag.StatoFatturaSelectList = Enum.GetValues(typeof(StatoProformaPreventivo))
                    .Cast<StatoProformaPreventivo>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString().Replace("InAttesaDiApprovazione", "In attesa di approvazione")
                    });
            ViewBag.ClienteId = new SelectList(db.Cliente.Where(c => c.UtenteId == utenteLoggato.UtenteId), "ClienteId", "NomeAzienda", documento.ClienteId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA.ToList(), "AliquotaIVAId", "Descrizione");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica(FatturaViewModel model, string RigheEliminate)
        {
            var utenteLoggato = GetUtenteLoggato();
            if (utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (ModelState.IsValid)
            {
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
                        if(rigaViewModel.RigaDocumentoId != 0)
                        {
                            var rigaDb = documentoDb.RigheDocumento.SingleOrDefault(r => r.RigaDocumentoId == rigaViewModel.RigaDocumentoId);
                            if (rigaDb != null)
                            {
                                rigaDb.Descrizione = rigaViewModel.Descrizione;
                                rigaDb.Quantita = rigaViewModel.Quantita;
                                rigaDb.PrezzoUnitario = rigaViewModel.PrezzoUnitario;
                                rigaDb.AliquotaIVAId = rigaViewModel.AliquotaIVAId;
                            }
                        } else
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
            ViewBag.StatoFatturaSelectList = Enum.GetValues(typeof(StatoProformaPreventivo))
                    .Cast<StatoProformaPreventivo>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString().Replace("InAttesaDiApprovazione", "In attesa di approvazione")
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

            return new Rotativa.ViewAsPdf("FatturaProformaPdf", viewModel)
            {
                FileName = $"Fattura_Proforma_{documento.Numero}.pdf"
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

            var pdf = new Rotativa.ViewAsPdf("FatturaProformaPdf", viewModel)
            {
                FileName = "FatturaProforma.pdf",
                CustomSwitches = "--no-stop-slow-scripts --javascript-delay 3000"
            };

            var binarioPdf = pdf.BuildFile(ControllerContext);

            using (var messaggio = new MailMessage())
            {
                messaggio.From = new MailAddress(emailMittente);
                messaggio.To.Add(new MailAddress(emailDestinatario));
                messaggio.CC.Add(new MailAddress(emailMittente));
                messaggio.Subject = "La tua fattura proforma";
                messaggio.Body = "Ecco la fattura proforma richiesta.";
                messaggio.Attachments.Add(new Attachment(new MemoryStream(binarioPdf), "FatturaProforma.pdf"));

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

                var emailDestinatario = documento.Cliente.Email; // send document to Client
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
