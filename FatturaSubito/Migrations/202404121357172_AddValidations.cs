namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValidations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Cliente", "Indirizzo", c => c.String(nullable: false));
            AlterColumn("dbo.Cliente", "Comune", c => c.String(nullable: false));
            AlterColumn("dbo.Cliente", "Provincia", c => c.String(nullable: false));
            AlterColumn("dbo.Cliente", "CAP", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Cliente", "CAP", c => c.String());
            AlterColumn("dbo.Cliente", "Provincia", c => c.String());
            AlterColumn("dbo.Cliente", "Comune", c => c.String());
            AlterColumn("dbo.Cliente", "Indirizzo", c => c.String());
        }
    }
}
