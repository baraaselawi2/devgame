using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace devgame.Models;

public partial class ModelContext : DbContext
{
    public ModelContext()
    {
    }

    public ModelContext(DbContextOptions<ModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("DATA SOURCE=localhost:1521/orcl;USER ID=C##DEV_GAME_USER;PASSWORD=12345aA@;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("C##DEV_GAME_USER")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008426");

            entity.ToTable("GAMES");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Bestquestionid)
                .HasColumnType("NUMBER")
                .HasColumnName("BESTQUESTIONID");
            entity.Property(e => e.Currentscore)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CURRENTSCORE");
            entity.Property(e => e.Playerid)
                .HasColumnType("NUMBER")
                .HasColumnName("PLAYERID");
            entity.Property(e => e.Timeended)
                .HasPrecision(6)
                .HasColumnName("TIMEENDED");
            entity.Property(e => e.Timestarted)
                .HasPrecision(6)
                .HasColumnName("TIMESTARTED");
            entity.Property(e => e.Totaltimespent)
                .HasColumnType("NUMBER")
                .HasColumnName("TOTALTIMESPENT");

            entity.HasOne(d => d.Player).WithMany(p => p.Games)
                .HasForeignKey(d => d.Playerid)
                .HasConstraintName("FK_GAME_PLAYER");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008424");

            entity.ToTable("PLAYERS");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Difficulty)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DIFFICULTY");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.Timestarted)
                .HasPrecision(6)
                .HasColumnName("TIMESTARTED");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008429");

            entity.ToTable("QUESTIONS");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Answer)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ANSWER");
            entity.Property(e => e.Gameid)
                .HasColumnType("NUMBER")
                .HasColumnName("GAMEID");
            entity.Property(e => e.Iscorrect)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISCORRECT");
            entity.Property(e => e.Questiontext)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("QUESTIONTEXT");
            entity.Property(e => e.Submittedanswer)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SUBMITTEDANSWER");
            entity.Property(e => e.Timesubmitted)
                .HasPrecision(6)
                .HasColumnName("TIMESUBMITTED");
            entity.Property(e => e.Timetaken)
                .HasColumnType("NUMBER")
                .HasColumnName("TIMETAKEN");

            entity.HasOne(d => d.Game).WithMany(p => p.Questions)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("FK_QUESTION_GAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
