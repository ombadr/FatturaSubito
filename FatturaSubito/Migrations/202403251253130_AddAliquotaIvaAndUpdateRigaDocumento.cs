namespace FatturaSubito.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAliquotaIvaAndUpdateRigaDocumento : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AliquotaIVA",
                c => new
                    {
                        AliquotaIVAId = c.Int(nullable: false, identity: true),
                        Descrizione = c.String(nullable: false),
                        ValorePercentuale = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.AliquotaIVAId);
            
            AddColumn("dbo.RigaDocumento", "AliquotaIVAId", c => c.Int(nullable: false));
            CreateIndex("dbo.RigaDocumento", "AliquotaIVAId");
            AddForeignKey("dbo.RigaDocumento", "AliquotaIVAId", "dbo.AliquotaIVA", "AliquotaIVAId", cascadeDelete: true);
            DropColumn("dbo.RigaDocumento", "AliquotaIVA");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RigaDocumento", "AliquotaIVA", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.RigaDocumento", "AliquotaIVAId", "dbo.AliquotaIVA");
            DropIndex("dbo.RigaDocumento", new[] { "AliquotaIVAId" });
            DropColumn("dbo.RigaDocumento", "AliquotaIVAId");
            DropTable("dbo.AliquotaIVA");
        }
    }
}
