using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestApplication.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RandomRow> RandomRows { get; set; } = null!;

        public void TruncateRandomRowsTable()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE \"RandomRows\"");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var host = ConfigurationManager.AppSettings["postgresql_host"];
                var port = ConfigurationManager.AppSettings["postgresql_port"];
                var db = ConfigurationManager.AppSettings["postgresql_database"];
                var user = ConfigurationManager.AppSettings["postgresql_username"];
                var password = ConfigurationManager.AppSettings["postgresql_password"];
                optionsBuilder.UseNpgsql($"Host={host};Port={port};Database={db};Username={user};Password={password}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RandomRow>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("RandomRows_pkey");

                entity.Property(e => e.EvenNumber).HasPrecision(8);

                entity.Property(e => e.FloatNumber).HasPrecision(10, 8);

                entity.Property(e => e.LatinString)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.RussianString)
                    .HasMaxLength(20)
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
