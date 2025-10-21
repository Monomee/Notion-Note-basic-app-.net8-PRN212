using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace NotionNote.Models;

public partial class NoteHubDbContext : DbContext
{
    public NoteHubDbContext()
    {
    }

    public NoteHubDbContext(DbContextOptions<NoteHubDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workspace> Workspaces { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured) { optionsBuilder.UseSqlServer(config.GetConnectionString("NoteHubDB")); }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.PageId).HasName("PK__Page__C565B104212244D4");

            entity.ToTable("Page");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPinned).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Workspace).WithMany(p => p.Pages)
                .HasForeignKey(d => d.WorkspaceId)
                .HasConstraintName("FK__Page__WorkspaceI__412EB0B6");

            entity.HasMany(d => d.Tags).WithMany(p => p.Pages)
                .UsingEntity<Dictionary<string, object>>(
                    "PageTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK__PageTag__TagId__47DBAE45"),
                    l => l.HasOne<Page>().WithMany()
                        .HasForeignKey("PageId")
                        .HasConstraintName("FK__PageTag__PageId__46E78A0C"),
                    j =>
                    {
                        j.HasKey("PageId", "TagId").HasName("PK__PageTag__13327E9E539B693B");
                        j.ToTable("PageTag");
                    });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tag__657CF9AC10194875");

            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "UQ__Tag__737584F6ECD604A8").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C4628922B");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E470E336EB").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.WorkspaceId).HasName("PK__Workspac__C84765D1338EF7AF");

            entity.ToTable("Workspace");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Workspaces)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Workspace__UserI__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
