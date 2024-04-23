namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cliente",
                c => new
                    {
                        ClienteId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Cognome = c.String(nullable: false),
                        NomeAzienda = c.String(nullable: false),
                        PartitaIVA = c.String(nullable: false),
                        CodiceFiscale = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Indirizzo = c.String(),
                        Comune = c.String(),
                        Provincia = c.String(),
                        CAP = c.String(),
                    })
                .PrimaryKey(t => t.ClienteId);
            
            CreateTable(
                "dbo.Documento",
                c => new
                    {
                        DocumentoId = c.Int(nullable: false, identity: true),
                        Tipo = c.Int(nullable: false),
                        Numero = c.Int(nullable: false),
                        Data = c.DateTime(nullable: false),
                        UtenteId = c.Int(nullable: false),
                        ClienteId = c.Int(),
                        FornitoreId = c.Int(),
                        Cestinato = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentoId)
                .ForeignKey("dbo.Cliente", t => t.ClienteId)
                .ForeignKey("dbo.Fornitore", t => t.FornitoreId)
                .ForeignKey("dbo.Utente", t => t.UtenteId, cascadeDelete: true)
                .Index(t => t.UtenteId)
                .Index(t => t.ClienteId)
                .Index(t => t.FornitoreId);
            
            CreateTable(
                "dbo.Fornitore",
                c => new
                    {
                        FornitoreId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Cognome = c.String(nullable: false),
                        NomeAzienda = c.String(nullable: false),
                        PartitaIVA = c.String(nullable: false),
                        CodiceFiscale = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Indirizzo = c.String(nullable: false),
                        Comune = c.String(),
                        Provincia = c.String(),
                        CAP = c.String(),
                    })
                .PrimaryKey(t => t.FornitoreId);
            
            CreateTable(
                "dbo.RigaDocumento",
                c => new
                    {
                        RigaDocumentoId = c.Int(nullable: false, identity: true),
                        Descrizione = c.String(),
                        Quantita = c.Int(nullable: false),
                        PrezzoUnitario = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AliquotaIVA = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DocumentoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RigaDocumentoId)
                .ForeignKey("dbo.Documento", t => t.DocumentoId, cascadeDelete: true)
                .Index(t => t.DocumentoId);
            
            CreateTable(
                "dbo.Utente",
                c => new
                    {
                        UtenteId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Cognome = c.String(nullable: false),
                        NomeAzienda = c.String(nullable: false),
                        PartitaIVA = c.String(nullable: false),
                        CodiceFiscale = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        Indirizzo = c.String(),
                        Comune = c.String(),
                        Provincia = c.String(),
                        CAP = c.String(),
                        Logo = c.String(),
                        Ruolo = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.UtenteId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Documento", "UtenteId", "dbo.Utente");
            DropForeignKey("dbo.RigaDocumento", "DocumentoId", "dbo.Documento");
            DropForeignKey("dbo.Documento", "FornitoreId", "dbo.Fornitore");
            DropForeignKey("dbo.Documento", "ClienteId", "dbo.Cliente");
            DropIndex("dbo.RigaDocumento", new[] { "DocumentoId" });
            DropIndex("dbo.Documento", new[] { "FornitoreId" });
            DropIndex("dbo.Documento", new[] { "ClienteId" });
            DropIndex("dbo.Documento", new[] { "UtenteId" });
            DropTable("dbo.Utente");
            DropTable("dbo.RigaDocumento");
            DropTable("dbo.Fornitore");
            DropTable("dbo.Documento");
            DropTable("dbo.Cliente");
        }
    }
}
