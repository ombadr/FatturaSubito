namespace FatturaSubito.Migrations
{
    using FatturaSubito.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Mvc;

    internal sealed class Configuration : DbMigrationsConfiguration<FatturaSubito.Models.DBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FatturaSubito.Models.DBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            if (!context.Set<AliquotaIVA>().Any())
            {
                var aliquoteIVA = new List<AliquotaIVA>
                {
                    new AliquotaIVA { Descrizione = "Esenzione 0%", ValorePercentuale = 0M},
                    new AliquotaIVA { Descrizione = "Ridotta 4%", ValorePercentuale = 4M },
                    new AliquotaIVA { Descrizione = "Ridotta 5%", ValorePercentuale = 5M },
                    new AliquotaIVA { Descrizione = "Ridotta 10%", ValorePercentuale = 10M },
                    new AliquotaIVA { Descrizione = "Ordinaria 22%", ValorePercentuale = 22M }
                };

                aliquoteIVA.ForEach(a => context.Set<AliquotaIVA>().Add(a));
                context.SaveChanges();
            }

            for (int i = 0; i < 10; i++)
            {
                context.Utente.AddOrUpdate(u => u.Email,
                    new Utente
                    {
                        Nome = $"Nome{i}",
                        Cognome = $"Cognome{i}",
                        NomeAzienda = $"Azienda{i}",
                        PartitaIVA = $"PIVA{i:00000000000}",
                        CodiceFiscale = $"CF{i:00000000000}",
                        Email = $"email{i}@example.com",
                        Password = $"password{i}",
                        Indirizzo = "Indirizzo",
                        Comune = "Comune",
                        Provincia = "Provincia",
                        CAP = "CAP",
                        Logo = null,
                        Attivo = true,
                        Ruolo = "User",
                        DataIscrizione = DateTime.Now,
                    });
            }
            context.SaveChanges();

            var utentiIds = context.Utente.Select(u => u.UtenteId).ToList();
            foreach (var utenteId in utentiIds)
            {
                context.Cliente.AddOrUpdate(c => c.Email,
                    new Cliente
                    {
                        UtenteId = utenteId,
                        Nome = $"ClienteNome{utenteId}",
                        Cognome = $"ClienteCognome{utenteId}",
                        NomeAzienda = $"ClienteAzienda{utenteId}",
                        PartitaIVA = $"CPIVA{utenteId:00000}",
                        CodiceFiscale = $"CCF{utenteId:00000}",
                        Email = $"cliente{utenteId}@example.com",
                        Indirizzo = "Indirizzo",
                        Comune = "Comune",
                        Provincia = "Provincia",
                        CAP = "CAP",
                    });

                context.Fornitore.AddOrUpdate(f => f.Email,
                    new Fornitore
                    {
                        UtenteId = utenteId,
                        Nome = $"FornitoreNome{utenteId}",
                        Cognome = $"FornitoreCognome{utenteId}",
                        NomeAzienda = $"FornitoreAzienda{utenteId}",
                        PartitaIVA = $"FPIVA{utenteId:00000}",
                        CodiceFiscale = $"FCF{utenteId:00000}",
                        Email = $"fornitore{utenteId}@example.com",
                        Indirizzo = "Indirizzo",
                        Comune = "Comune",
                        Provincia = "Provincia",
                        CAP = "CAP",
                    });
            }

            context.SaveChanges();

            

            // var utentiIds = context.Utente.Select(u => u.UtenteId).ToList();
            var clientiIds = context.Cliente.Select(c => c.ClienteId).ToList();
            var fornitoriIds = context.Fornitore.Select(f => f.FornitoreId).ToList();
            var aliquotaIvaIds = context.AliquotaIVA.Select(a => a.AliquotaIVAId).ToList();

            var random = new Random();

            foreach (var utenteId in utentiIds)
            {
                for (int i = 0; i < 5; i++) // generate 5 docs for user
                {
                    var tipoDocumento = (TipoDocumento)random.Next(0, 5);
                    var clienteCasuale = clientiIds[random.Next(clientiIds.Count)];
                    var fornitoreCasuale = fornitoriIds[random.Next(fornitoriIds.Count)];


                    var documento = new Documento
                    {
                        Tipo = tipoDocumento,
                        Numero = i + 1,
                        Data = DateTime.Now.AddDays(-random.Next(0, 365)),
                        UtenteId = utenteId,
                        ClienteId = (tipoDocumento == TipoDocumento.FatturaVendita || tipoDocumento == TipoDocumento.Proforma || tipoDocumento == TipoDocumento.Preventivo || tipoDocumento == TipoDocumento.BollaDiTrasporto)
                                     ? clienteCasuale : (int?)null,
                        FornitoreId = tipoDocumento == TipoDocumento.FatturaAcquisto ? fornitoreCasuale : (int?)null,
                        Cestinato = random.Next(100) < 20,
                        RigheDocumento = new List<RigaDocumento>(),
                        TerminiCondizioni = "Hello"
                    };

                    int numeroRighe = random.Next(1, 6);
                    for (int r = 0; r < numeroRighe; r++)
                    {
                        var aliquotaIvaCasualeId = aliquotaIvaIds[random.Next(aliquotaIvaIds.Count)];
                        documento.RigheDocumento.Add(new RigaDocumento
                        {
                            Descrizione = $"Prodotto {r + 1}",
                            Quantita = random.Next(1, 11),
                            PrezzoUnitario = random.Next(10, 101),
                            AliquotaIVAId = aliquotaIvaCasualeId
                        });
                    }

                    context.Documento.AddOrUpdate(d => new { d.Numero, d.UtenteId }, documento);
                }
            }

            context.SaveChanges();
        }
    }
}
