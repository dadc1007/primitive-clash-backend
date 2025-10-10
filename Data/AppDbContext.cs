using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Card> Cards { get; set; }
        public DbSet<PlayerCard> PlayerCards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<Enum>()
                .HaveConversion<string>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Card
            modelBuilder.Entity<Card>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Card>()
                .HasDiscriminator<string>("CardType")
                .HasValue<SpellCard>("Spell")
                .HasValue<TroopCard>("Troop")
                .HasValue<BuildingCard>("Building");

            modelBuilder.Entity<Card>()
                .Property(c => c.Targets)
                .HasConversion(
                    v => v.Select(e => e.ToString()).ToArray(),
                    v => v.Select(e => Enum.Parse<CardTarget>(e)).ToList()
                )
                .HasColumnType("text[]")
                .Metadata.SetValueComparer(
                    new ValueComparer<List<CardTarget>>(
                        (c1, c2) => c1!.OrderBy(e => e).SequenceEqual(c2!.OrderBy(e => e)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            // PlayerCard
            modelBuilder.Entity<PlayerCard>()
                .HasOne(pc => pc.Card)
                .WithMany()
                .HasForeignKey(pc => pc.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Deck
            modelBuilder.Entity<Deck>()
                .HasMany(d => d.PlayerCards)
                .WithOne()
                .HasForeignKey(pc => pc.DeckId);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.PlayerCards)
                .WithOne()
                .HasForeignKey(pc => pc.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Deck)
                .WithOne()
                .HasForeignKey<User>(u => u.DeckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}