using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SurePath.Markets.Models
{
    public class MarketsDbContext : IdentityDbContext<ApplicationUser>
    {
        public MarketsDbContext(DbContextOptions<MarketsDbContext> options)
            : base(options) { }

        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeScreenshot> TradeScreenshots { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Trade>(t =>
            {
                t.HasOne(x => x.User)
                 .WithMany(u => u.Trades)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                t.Property(x => x.Entry).HasColumnType("decimal(18,4)");
                t.Property(x => x.Exit).HasColumnType("decimal(18,4)");
                t.Property(x => x.StopLoss).HasColumnType("decimal(18,4)");
                t.Property(x => x.Target).HasColumnType("decimal(18,4)");
                t.Property(x => x.PnL).HasColumnType("decimal(18,4)");
                t.Property(x => x.RRRatio).HasColumnType("decimal(18,4)");
            });

            builder.Entity<TradeScreenshot>(s =>
            {
                s.HasOne(x => x.Trade)
                 .WithMany(t => t.Screenshots)
                 .HasForeignKey(x => x.TradeId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}