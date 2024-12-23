using Microsoft.EntityFrameworkCore;
using TaxService.Models;

namespace TaxService.Data;

public partial class ApplicationDbContext: DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Taxpayer> Taxpayers { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Documentid).HasName("documents_pkey");

            entity.ToTable("documents");

            entity.Property(e => e.Documentid).HasColumnName("documentid");
            entity.Property(e => e.Filepath).HasColumnName("filepath");
            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");

            entity.HasOne(d => d.Report).WithMany(p => p.Documents)
                .HasForeignKey(d => d.Reportid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("documents_reportid_fkey");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("logs_pkey");

            entity.ToTable("logs");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.Actiondate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actiondate");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("logs_userid_fkey");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Reportid).HasName("reports_pkey");

            entity.ToTable("reports");

            entity.Property(e => e.Reportid).HasColumnName("reportid");
            entity.Property(e => e.Errordescription).HasColumnName("errordescription");
            entity.Property(e => e.Reporttype)
                .HasMaxLength(100)
                .HasColumnName("reporttype");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Отправлен'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Submissiondate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("submissiondate");
            entity.Property(e => e.Taxpayerid).HasColumnName("taxpayerid");

            entity.HasOne(d => d.Taxpayer).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Taxpayerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reports_taxpayerid_fkey");
        });

        modelBuilder.Entity<Taxpayer>(entity =>
        {
            entity.HasKey(e => e.Taxpayerid).HasName("taxpayers_pkey");

            entity.ToTable("taxpayers");

            entity.HasIndex(e => e.Inn, "taxpayers_inn_key").IsUnique();

            entity.Property(e => e.Taxpayerid).HasColumnName("taxpayerid");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Companyname)
                .HasMaxLength(255)
                .HasColumnName("companyname");
            entity.Property(e => e.Inn)
                .HasMaxLength(10)
                .HasColumnName("inn");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Taxpayers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("taxpayers_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}