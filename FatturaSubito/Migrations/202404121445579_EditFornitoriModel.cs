namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditFornitoriModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Fornitore", "Comune", c => c.String(nullable: false));
            AlterColumn("dbo.Fornitore", "Provincia", c => c.String(nullable: false));
            AlterColumn("dbo.Fornitore", "CAP", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Fornitore", "CAP", c => c.String());
            AlterColumn("dbo.Fornitore", "Provincia", c => c.String());
            AlterColumn("dbo.Fornitore", "Comune", c => c.String());
        }
    }
}
