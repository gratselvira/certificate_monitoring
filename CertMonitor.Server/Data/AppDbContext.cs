using CertMonitor.Server.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workstation> Workstations => Set<Workstation>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<TokenDevice> TokenDevices => Set<TokenDevice>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<ScanSession> ScanSessions => Set<ScanSession>();

    public DbSet<CertificateDetection> CertificateDetections => Set<CertificateDetection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Workstation>(entity =>
        {
            entity.ToTable("Workstations");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DeviceUid).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Hostname).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FirstSeenAtUtc).IsRequired();
            entity.Property(x => x.LastSeenAtUtc).IsRequired();

            entity.HasIndex(x => x.DeviceUid).IsUnique();

            entity.HasOne(x => x.Agent)
                .WithOne(x => x.Workstation)
                .HasForeignKey<Agent>(x => x.WorkstationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AgentVersion).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LastScanAtUtc).IsRequired();

            entity.HasIndex(x => x.WorkstationId).IsUnique();
        });

        modelBuilder.Entity<TokenDevice>(entity =>
        {
            entity.ToTable("TokenDevices");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SerialNumber).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TokenType).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Pkcs11SlotId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FirstSeenAtUtc).IsRequired();
            entity.Property(x => x.LastSeenAtUtc).IsRequired();

            entity.HasIndex(x => x.SerialNumber).IsUnique();

            entity.HasOne(x => x.Workstation)
                .WithMany(x => x.TokenDevices)
                .HasForeignKey(x => x.WorkstationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.ToTable("Certificates");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Thumbprint).HasMaxLength(256);
            entity.Property(x => x.Issuer).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SerialNumber).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Algorithm).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FirstSeenAtUtc).IsRequired();
            entity.Property(x => x.LastSeenAtUtc).IsRequired();
            entity.Property(x => x.ValidFromUtc).IsRequired();
            entity.Property(x => x.ValidToUtc).IsRequired();

            entity.HasIndex(x => x.Thumbprint)
                .IsUnique()
                .HasFilter("\"Thumbprint\" IS NOT NULL AND \"Thumbprint\" <> ''");

            entity.HasIndex(x => new { x.Issuer, x.SerialNumber })
                .IsUnique()
                .HasFilter("\"Thumbprint\" IS NULL OR \"Thumbprint\" = ''");

            entity.HasOne(x => x.TokenDevice)
                .WithMany(x => x.Certificates)
                .HasForeignKey(x => x.TokenDeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScanSession>(entity =>
        {
            entity.ToTable("ScanSessions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SessionUid).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RawPayloadJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.StartedAtUtc).IsRequired();
            entity.Property(x => x.FinishedAtUtc).IsRequired();
            entity.Property(x => x.TokensFoundCount).IsRequired();
            entity.Property(x => x.CertificatesFoundCount).IsRequired();

            entity.HasIndex(x => x.SessionUid).IsUnique();

            entity.HasOne(x => x.Workstation)
                .WithMany(x => x.ScanSessions)
                .HasForeignKey(x => x.WorkstationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Agent)
                .WithMany(x => x.ScanSessions)
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CertificateDetection>(entity =>
        {
            entity.ToTable("CertificateDetections");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DetectedAtUtc).IsRequired();

            entity.HasOne(x => x.ScanSession)
                .WithMany(x => x.CertificateDetections)
                .HasForeignKey(x => x.ScanSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Certificate)
                .WithMany(x => x.CertificateDetections)
                .HasForeignKey(x => x.CertificateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TokenDevice)
                .WithMany(x => x.CertificateDetections)
                .HasForeignKey(x => x.TokenDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ScanSessionId, x.CertificateId, x.TokenDeviceId }).IsUnique();
        });
    }
}
