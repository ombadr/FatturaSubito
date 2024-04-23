namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTerminiCondizioniDocumentoModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documento", "TerminiCondizioni", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documento", "TerminiCondizioni");
        }
    }
}
