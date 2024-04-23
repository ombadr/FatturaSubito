using FatturaSubito.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor;

namespace FatturaSubito.Controllers
{
    public class FattureAcquistoController : Controller
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
        // GET: FattureAcquisto
        [Authorize]
        public ActionResult Index()
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato != null)
            {
                var documenti = db.Documento
                    .Include("RigheDocumento.AliquotaIVA")
                    .Where(d => d.UtenteId == utenteLoggato.UtenteId && d.Tipo == TipoDocumento.FatturaAcquisto && d.Cestinato == false)
                    .ToList();

                var documentiViewModel = documenti.Select(d => new DocumentoViewModel
                {
                    Id = d.DocumentoId,
                    Numero = d.Numero,
                    Data = d.Data,
                    NomeAzienda = d.Fornitore.NomeAzienda,
                    Email = d.Fornitore.Email,
                    TotaleDocumento = d.RigheDocumento.Sum(rd => rd.Quantita * rd.PrezzoUnitario * (1 + rd.AliquotaIVA.ValorePercentuale / 100)),
                    Stato = GetStatoFatturaDisplayString((StatoFattura)d.Stato)
                }).ToList();

                return View(documentiViewModel);
            } else
            {
                return RedirectToAction("Login", "Auth");
            }

        }
        [Authorize]
        public ActionResult Aggiungi()
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato != null)
            {
                var viewModel = new FatturaViewModel
                {
                    Documento = new Documento
                    {
                        Tipo = TipoDocumento.FatturaAcquisto
                    },
                    Utente = utenteLoggato
                };
                ViewBag.FornitoreId = new SelectList(db.Fornitore.Where(f => f.UtenteId == utenteLoggato.UtenteId), "FornitoreId", "NomeAzienda");
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
                foreach(var riga in model.RigheDocumento)
                {
                    riga.DocumentoId = model.Documento.DocumentoId;
                    db.RigaDocumento.Add(riga);
                }
                db.SaveChanges();
                return RedirectToAction("Index"); 
            }

            ViewBag.FornitoreId = new SelectList(db.Fornitore.Where(f => f.UtenteId == utenteLoggato.UtenteId), "FornitoreId", "NomeAzienda", model.Documento.FornitoreId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA, "AliquotaIVAId", "Descrizione");
            return View(model);
        }
        [Authorize]
        public ActionResult DettagliFornitore(int id)
        {
            var fornitore = db.Fornitore.Find(id);

            if (fornitore == null)
            {
                return HttpNotFound();
            }

            var fornitoreView = new
            {
                Nome = fornitore.Nome,
                Cognome = fornitore.Cognome,
                NomeAzienda = fornitore.NomeAzienda,
                Indirizzo = fornitore.Indirizzo,
                CAP = fornitore.CAP,
                Comune = fornitore.Comune,
                Provincia = fornitore.Provincia,
                PartitaIVA = fornitore.PartitaIVA,
                CodiceFiscale = fornitore.CodiceFiscale,
            };

            return Json(fornitoreView, JsonRequestBehavior.AllowGet);
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

            if(utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var documento = db.Documento
                .Include("RigheDocumento.AliquotaIVA")
                .SingleOrDefault(d => d.DocumentoId == id && d.UtenteId == utenteLoggato.UtenteId);

            if(documento.UtenteId != utenteLoggato.UtenteId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            }

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
            ViewBag.FornitoreId = new SelectList(db.Fornitore.Where(c => c.UtenteId == utenteLoggato.UtenteId), "FornitoreId", "NomeAzienda", documento.FornitoreId);
            ViewBag.AliquotaIVAId = new SelectList(db.AliquotaIVA.ToList(), "AliquotaIVAId", "Descrizione");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modifica(FatturaViewModel model, string RigheEliminate)
        {
            var utenteLoggato = GetUtenteLoggato();
            if(utenteLoggato == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if(ModelState.IsValid)
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
            ViewBag.FornitoreId = new SelectList(db.Fornitore.Where(c => c.UtenteId == utenteLoggato.UtenteId), "FornitoreId", "NomeAzienda", model.Documento.FornitoreId);
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

            return new Rotativa.ViewAsPdf("FatturaAcquistoPdf", viewModel)
            {
                FileName = $"Fattura_Acquisto_{documento.Numero}.pdf",
                CustomSwitches = "--no-stop-slow-scripts --javascript-delay 3000"
            };
        }
    }
}