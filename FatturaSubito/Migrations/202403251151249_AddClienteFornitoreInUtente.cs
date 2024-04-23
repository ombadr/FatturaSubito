namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClienteFornitoreInUtente : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cliente", "UtenteId", c => c.Int(nullable: false));
            AddColumn("dbo.Fornitore", "UtenteId", c => c.Int(nullable: false));
            CreateIndex("dbo.Cliente", "UtenteId");
            CreateIndex("dbo.Fornitore", "UtenteId");
            AddForeignKey("dbo.Cliente", "UtenteId", "dbo.Utente", "UtenteId", cascadeDelete: true);
            AddForeignKey("dbo.Fornitore", "UtenteId", "dbo.Utente", "UtenteId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Fornitore", "UtenteId", "dbo.Utente");
            DropForeignKey("dbo.Cliente", "UtenteId", "dbo.Utente");
            DropIndex("dbo.Fornitore", new[] { "UtenteId" });
            DropIndex("dbo.Cliente", new[] { "UtenteId" });
            DropColumn("dbo.Fornitore", "UtenteId");
            DropColumn("dbo.Cliente", "UtenteId");
        }
    }
}
