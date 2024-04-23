namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiStatoUtente : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Utente", "Attivo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Utente", "Attivo");
        }
    }
}
