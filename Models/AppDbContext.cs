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

        public virtual DbSet<AccountStatement> AccountStatements { get; set; } = null!;
        public virtual DbSet<AccountingView> AccountingView { get; set; } = null!;
        public virtual DbSet<RandomRow> RandomRows { get; set; } = null!;
        public virtual DbSet<TurnoverSheet> TurnoverSheets { get; set; } = null!;
        public virtual DbSet<TurnoverStatement> TurnoverStatements { get; set; } = null!;

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
            modelBuilder.Entity<AccountStatement>(entity =>
            {
                entity.HasKey(e => e.Statement)
                    .HasName("PK_ACCOUNT_STATEMENT");

                entity.ToTable("AccountStatement");

                entity.Property(e => e.AccountType).HasMaxLength(7);

                entity.Property(e => e.OpeningBalance).HasPrecision(20, 2);

                entity.HasOne(d => d.StatementNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.Statement)
                    .HasConstraintName("FK_ACCOUNTING_TURNOVER_STATEMENT");
            });

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

            modelBuilder.Entity<TurnoverSheet>(entity =>
            {
                entity.HasKey(e => e.SheetId)
                    .HasName("PK_TURNOVERSHEET_SHEETID");

                entity.ToTable("TurnoverSheet");

                entity.Property(e => e.BankName).HasMaxLength(100);

                entity.Property(e => e.Currency).HasMaxLength(10);

                entity.Property(e => e.FileName).HasMaxLength(260);
            });

            modelBuilder.Entity<TurnoverStatement>(entity =>
            {
                entity.HasKey(e => e.StatementId)
                    .HasName("PK_TURNOVER_STATEMENT_ID");

                entity.ToTable("TurnoverStatement");

                entity.HasIndex(e => new { e.AccountCode, e.TurnoverSheet }, "UNIQUE_ACCOUNT_SHEET_COMBINATION")
                    .IsUnique();

                entity.Property(e => e.AccountCode).HasPrecision(4);

                entity.Property(e => e.Credit).HasPrecision(20, 2);

                entity.Property(e => e.Debit).HasPrecision(20, 2);

                entity.HasOne(d => d.TurnoverSheetNavigation)
                    .WithMany(p => p.TurnoverStatements)
                    .HasForeignKey(d => d.TurnoverSheet)
                    .HasConstraintName("FK_ACCOUNTING_TURNOVERSHEET");
            });
            modelBuilder.Entity<AccountingView>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.Code).HasColumnName("AccountCode");
                entity.Property(e => e.TurnoverSheetId).HasColumnName("TurnoverSheet");
                entity.ToView("AccountingView");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
