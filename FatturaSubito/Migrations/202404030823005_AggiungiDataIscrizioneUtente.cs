namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiDataIscrizioneUtente : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Utente", "DataIscrizione", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Utente", "DataIscrizione");
        }
    }
}
