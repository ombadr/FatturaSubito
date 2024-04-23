namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AggiungiStatoDocumento : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documento", "StatoValore", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documento", "StatoValore");
        }
    }
}
