using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SurePath.Markets.Models
{
    public class Trade
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Symbol { get; set; }
        public DateTime TradeDate { get; set; }
        public string Direction { get; set; }   // long / short
        public string AssetType { get; set; }   // futures / options / stock
        public string Timeframe { get; set; }
        public string Status { get; set; }       // open / closed / stopped

        public decimal Entry { get; set; }
        public decimal? Exit { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? Target { get; set; }
        public int Contracts { get; set; } = 1;

        public decimal? PnL { get; set; }
        public decimal? RRRatio { get; set; }
        public string Setup { get; set; }
        public string Notes { get; set; }
        public string Emotions { get; set; }    // stored as comma-separated

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TradeScreenshot> Screenshots { get; set; }
    }

    public class TradeScreenshot
    {
        public int Id { get; set; }
        public int TradeId { get; set; }
        public Trade Trade { get; set; }
        public string DataUrl { get; set; }
        public string Label { get; set; }
        public string FileName { get; set; }
        public int SortOrder { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Trade> Trades { get; set; }
    }
}