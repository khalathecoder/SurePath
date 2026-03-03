using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurePath.Markets.Models;

namespace SurePath.Markets.Services;

public static class SeedService
{
    // ── Credentials ───────────────────────────────────────────────────────────
    // Change AdminPassword via environment variable or user-secrets in production.
    private const string AdminEmail    = "admin@spe.com";
    private const string AdminPassword = "Admin@1234!";

    private const string DemoEmail    = "demo@spe.com";
    private const string DemoPassword = "Demo@1234!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db          = services.GetRequiredService<MarketsDbContext>();

        // ── 1. Roles ──────────────────────────────────────────────────────────
        string[] roles = ["Admin", "Trader", "Demo"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── 2. Admin user ──────────────────────────────────────────────────────
        if (await userManager.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName       = AdminEmail,
                Email          = AdminEmail,
                DisplayName    = "Admin",
                JoinedAt       = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ── 3. Demo user ───────────────────────────────────────────────────────
        var demoUser = await userManager.FindByEmailAsync(DemoEmail);
        if (demoUser is null)
        {
            demoUser = new ApplicationUser
            {
                UserName       = DemoEmail,
                Email          = DemoEmail,
                DisplayName    = "Demo Trader",
                JoinedAt       = DateTime.UtcNow.AddMonths(-3),
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(demoUser, DemoPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(demoUser, "Demo");
        }

        // ── 4. Demo trades (only seed once) ───────────────────────────────────
        if (!await db.Trades.AnyAsync(t => t.UserId == demoUser.Id))
        {
            var uid = demoUser.Id;
            var now = DateTime.UtcNow;

            var trades = new List<Trade>
            {
                // ── Week 1 (most recent) ──────────────────────────────────────

                // Open trade — currently running
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-1),
                    Direction="long",  AssetType="futures", Timeframe="4h",
                    Entry=21380m, StopLoss=21280m, Target=21680m, Contracts=1,
                    Status="open", Setup="Demand Zone",
                    Notes="Weekly demand zone test, holding with conviction",
                    Emotions="patient", CreatedAt=now.AddDays(-1) },

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-2),
                    Direction="long",  AssetType="futures", Timeframe="5m",
                    Entry=21450m, Exit=21592m, StopLoss=21380m, Target=21590m,
                    Contracts=2, Status="closed", PnL=2840m, RRRatio=2.03m,
                    Setup="Bull Flag",
                    Notes="Clean breakout above VWAP, strong momentum into NY close",
                    Emotions="focused,confident", CreatedAt=now.AddDays(-2) },

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-3),
                    Direction="short", AssetType="futures", Timeframe="15m",
                    Entry=21678m, Exit=21560m, StopLoss=21742m, Target=21550m,
                    Contracts=1, Status="closed", PnL=1180m, RRRatio=1.84m,
                    Setup="Rejection at Resistance",
                    Notes="Double top at key level, clean entry on second touch",
                    Emotions="patient,focused", CreatedAt=now.AddDays(-3) },

                // Stop — same day, revenge trade
                new() { UserId=uid, Symbol="ES", TradeDate=now.AddDays(-3),
                    Direction="long",  AssetType="futures", Timeframe="5m",
                    Entry=5842m, Exit=5832m, StopLoss=5832m, Target=5872m,
                    Contracts=2, Status="stopped", PnL=-2000m, RRRatio=-1.0m,
                    Setup="VWAP Reclaim",
                    Notes="Failed to hold VWAP, stopped out quickly. Should have waited for confirmation",
                    Emotions="frustrated,impulsive", CreatedAt=now.AddDays(-3).AddHours(2) },

                // ── Week 2 ────────────────────────────────────────────────────

                // Win — big R
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-7),
                    Direction="long",  AssetType="futures", Timeframe="1h",
                    Entry=21200m, Exit=21352m, StopLoss=21148m, Target=21352m,
                    Contracts=2, Status="closed", PnL=3040m, RRRatio=2.92m,
                    Setup="Breakout Retest",
                    Notes="Perfect retest of breakout level, held overnight",
                    Emotions="confident,calm", CreatedAt=now.AddDays(-7) },

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-8),
                    Direction="short", AssetType="futures", Timeframe="5m",
                    Entry=21798m, Exit=21700m, StopLoss=21850m, Target=21694m,
                    Contracts=2, Status="closed", PnL=1960m, RRRatio=1.88m,
                    Setup="Failed Breakout",
                    Notes="Lost range highs, trapped longs provided momentum",
                    Emotions="focused", CreatedAt=now.AddDays(-8) },

                // Stop
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-9),
                    Direction="long",  AssetType="futures", Timeframe="15m",
                    Entry=21305m, Exit=21255m, StopLoss=21255m, Target=21455m,
                    Contracts=1, Status="stopped", PnL=-750m, RRRatio=-1.5m,
                    Setup="Morning Reversal",
                    Notes="Looked good at open but reversed hard, waited too long for entry",
                    Emotions="anxious,fomo", CreatedAt=now.AddDays(-9) },

                // ── Week 3 ────────────────────────────────────────────────────

                // Win — best trade of the batch
                new() { UserId=uid, Symbol="ES", TradeDate=now.AddDays(-14),
                    Direction="long",  AssetType="futures", Timeframe="5m",
                    Entry=5878m, Exit=5912m, StopLoss=5868m, Target=5908m,
                    Contracts=3, Status="closed", PnL=5100m, RRRatio=3.0m,
                    Setup="Open Range Breakout",
                    Notes="ORB with above-average volume, textbook execution",
                    Emotions="focused,confident", CreatedAt=now.AddDays(-14) },

                // Win — Gold
                new() { UserId=uid, Symbol="GC", TradeDate=now.AddDays(-15),
                    Direction="long",  AssetType="futures", Timeframe="1h",
                    Entry=3318m, Exit=3342m, StopLoss=3308m, Target=3348m,
                    Contracts=1, Status="closed", PnL=2400m, RRRatio=2.4m,
                    Setup="Trend Continuation",
                    Notes="Gold strong trend, entered on pullback to 20 EMA",
                    Emotions="calm,patient", CreatedAt=now.AddDays(-15) },

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-16),
                    Direction="short", AssetType="futures", Timeframe="15m",
                    Entry=21998m, Exit=21878m, StopLoss=22058m, Target=21878m,
                    Contracts=2, Status="closed", PnL=2400m, RRRatio=2.0m,
                    Setup="Distribution",
                    Notes="Supply zone absorption, clean flush through prior low",
                    Emotions="focused", CreatedAt=now.AddDays(-16) },

                // ── Week 4 ────────────────────────────────────────────────────

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-21),
                    Direction="long",  AssetType="futures", Timeframe="5m",
                    Entry=21102m, Exit=21250m, StopLoss=21052m, Target=21252m,
                    Contracts=1, Status="closed", PnL=1480m, RRRatio=2.96m,
                    Setup="Gap Fill",
                    Notes="Gap from prior day filled cleanly, let runners go",
                    Emotions="focused,calm", CreatedAt=now.AddDays(-21) },

                // Stop — big loss, overtrading
                new() { UserId=uid, Symbol="ES", TradeDate=now.AddDays(-22),
                    Direction="short", AssetType="futures", Timeframe="1h",
                    Entry=5902m, Exit=5922m, StopLoss=5922m, Target=5842m,
                    Contracts=2, Status="stopped", PnL=-4000m, RRRatio=-1.0m,
                    Setup="Head & Shoulders",
                    Notes="Pattern invalidated, took full stop. Overtraded after the morning session",
                    Emotions="frustrated,revenge", CreatedAt=now.AddDays(-22) },

                // Win — recovery
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-23),
                    Direction="long",  AssetType="futures", Timeframe="1h",
                    Entry=20798m, Exit=20950m, StopLoss=20728m, Target=21008m,
                    Contracts=1, Status="closed", PnL=1520m, RRRatio=2.17m,
                    Setup="Monthly Level Bounce",
                    Notes="Clean hold of major monthly support, added at confirmation",
                    Emotions="confident,patient", CreatedAt=now.AddDays(-23) },

                // ── Week 5+ ───────────────────────────────────────────────────

                // Win
                new() { UserId=uid, Symbol="NQ", TradeDate=now.AddDays(-30),
                    Direction="long",  AssetType="futures", Timeframe="15m",
                    Entry=20542m, Exit=20688m, StopLoss=20472m, Target=20752m,
                    Contracts=2, Status="closed", PnL=2920m, RRRatio=2.09m,
                    Setup="Bull Flag",
                    Notes="Strong momentum day, multiple flag entries",
                    Emotions="focused,confident", CreatedAt=now.AddDays(-30) },
            };

            db.Trades.AddRange(trades);
            await db.SaveChangesAsync();
        }
    }
}
