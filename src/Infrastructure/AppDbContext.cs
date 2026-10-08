    using Microsoft.EntityFrameworkCore;
    using PruebaConceptoRabbitMQ.Domain;

    namespace PruebaConceptoRabbitMQ.Infrastructure;

    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TransferenciaStp> TransferenciasStp => Set<TransferenciaStp>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransferenciaStp>(entity =>
            {
                entity.ToTable("transferenciaStp");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Monto).HasColumnType("MONEY");
                entity.Property(e => e.ClaveRastreo).HasMaxLength(50);
                entity.Property(e => e.BancoEmisor).HasMaxLength(100);
                entity.Property(e => e.BancoReceptor).HasMaxLength(100);
                entity.Property(e => e.CunetaBeneficiar).HasMaxLength(50);
            });
        }
    }