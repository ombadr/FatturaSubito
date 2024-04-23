using System;
using System.Data.Entity;
using System.Linq;

namespace FatturaSubito.Models
{
    public class DBContext : DbContext
    {
        // Your context has been configured to use a 'DBContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'FatturaSubito.Models.DBContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DBContext' 
        // connection string in the application configuration file.
        public DBContext()
            : base("name=DBContext.cs")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Documento> Documento { get; set; }
        public DbSet<Fornitore> Fornitore { get; set; }
        public DbSet<Utente> Utente { get; set; }
        public DbSet<RigaDocumento> RigaDocumento { get; set; }
        public DbSet<AliquotaIVA> AliquotaIVA { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}