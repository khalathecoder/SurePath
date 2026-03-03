using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurePath.Markets.Models;

namespace SurePath.Markets.Controllers
{
    public class TradeController : Controller
    {
        private readonly MarketsDbContext                _db;
        private readonly UserManager<ApplicationUser>   _userManager;
        private readonly SignInManager<ApplicationUser> _signIn;

        public TradeController(
            MarketsDbContext                db,
            UserManager<ApplicationUser>   userManager,
            SignInManager<ApplicationUser> signIn)
        {
            _db          = db;
            _userManager = userManager;
            _signIn      = signIn;
        }

        // ── VIEWS ─────────────────────────────────────────────────────────────
        // Anonymous visitors are automatically signed in as the demo user so
        // they land directly on the journal without hitting the login wall.

        [AllowAnonymous]
        public async Task<IActionResult> Journal()
        {
            await EnsureDemoSessionAsync();
            ViewData["ActiveNav"] = "journal";
            ViewData["Division"]  = "Trading Desk";
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> RiskCalc()
        {
            await EnsureDemoSessionAsync();
            ViewData["ActiveNav"] = "riskcalc";
            ViewData["Division"]  = "Trading Desk";
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Analytics()
        {
            await EnsureDemoSessionAsync();
            ViewData["ActiveNav"] = "analytics";
            ViewData["Division"]  = "Trading Desk";
            return View();
        }

        // ── API: GET trades ───────────────────────────────────────────────────
        // After auto-sign-in above the user will have a valid session,
        // so [Authorize] here is safe — it will never redirect to login mid-page.
        [HttpGet, Authorize]
        public async Task<IActionResult> GetTrades()
        {
            var userId = _userManager.GetUserId(User);
            var trades = await _db.Trades
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    id        = t.Id,
                    sym       = t.Symbol,
                    date      = t.TradeDate.ToString("yyyy-MM-dd"),
                    dir       = t.Direction,
                    type      = t.AssetType,
                    entry     = t.Entry,
                    exit      = t.Exit,
                    stop      = t.StopLoss,
                    target    = t.Target,
                    size      = t.Contracts,
                    status    = t.Status,
                    setup     = t.Setup,
                    tf        = t.Timeframe,
                    notes     = t.Notes,
                    emotions  = t.Emotions,
                    pnl       = t.PnL,
                    rr        = t.RRRatio,
                    createdAt = t.CreatedAt
                })
                .ToListAsync();

            return Json(trades);
        }

        // ── API: POST trade ───────────────────────────────────────────────────
        // Demo role is intentionally excluded — read-only access.
        [HttpPost, Authorize(Roles = "Admin,Trader")]
        public async Task<IActionResult> SaveTrade([FromBody] TradeDto dto)
        {
            var userId = _userManager.GetUserId(User);

            var trade = new Trade
            {
                UserId    = userId,
                Symbol    = dto.sym?.ToUpper(),
                TradeDate = DateTime.TryParse(dto.date, out var d) ? d : DateTime.UtcNow,
                Direction = dto.dir,
                AssetType = dto.type,
                Entry     = dto.entry,
                Exit      = dto.exit,
                StopLoss  = dto.stop,
                Target    = dto.target,
                Contracts = dto.size > 0 ? dto.size : 1,
                Status    = dto.status ?? "open",
                Setup     = dto.setup,
                Timeframe = dto.tf,
                Notes     = dto.notes,
                Emotions  = dto.emotions,
                PnL       = dto.pnl,
                RRRatio   = dto.rr,
                CreatedAt = DateTime.UtcNow
            };

            _db.Trades.Add(trade);
            await _db.SaveChangesAsync();

            return Json(new { success = true, id = trade.Id });
        }

        // ── API: DELETE trade ─────────────────────────────────────────────────
        [HttpDelete, Authorize(Roles = "Admin,Trader")]
        public async Task<IActionResult> DeleteTrade(int id)
        {
            var userId = _userManager.GetUserId(User);
            var trade  = await _db.Trades
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trade == null) return NotFound();

            _db.Trades.Remove(trade);
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// If the visitor has no session, silently sign them in as the demo user.
        /// This makes every public entry point land directly on real content.
        /// </summary>
        private async Task EnsureDemoSessionAsync()
        {
            if (_signIn.IsSignedIn(User)) return;

            var demo = await _userManager.FindByEmailAsync("demo@spe.com");
            if (demo is not null)
                await _signIn.SignInAsync(demo, isPersistent: false);
        }
    }

    // ── DTO ───────────────────────────────────────────────────────────────────
    public class TradeDto
    {
        public string   sym      { get; set; }
        public string   date     { get; set; }
        public string   dir      { get; set; }
        public string   type     { get; set; }
        public decimal  entry    { get; set; }
        public decimal? exit     { get; set; }
        public decimal? stop     { get; set; }
        public decimal? target   { get; set; }
        public int      size     { get; set; }
        public string   status   { get; set; }
        public string   setup    { get; set; }
        public string   tf       { get; set; }
        public string   notes    { get; set; }
        public string   emotions { get; set; }
        public decimal? pnl      { get; set; }
        public decimal? rr       { get; set; }
    }
}
