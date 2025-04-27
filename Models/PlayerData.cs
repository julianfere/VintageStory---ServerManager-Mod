using System;

namespace ServerManager.Models
{
    public class PlayerData
    {
        public string PlayerUID { get; set; } = string.Empty;
        public string Name { get; set; }
        public DateTime LastLogin { get; set; } = DateTime.MinValue;
        public DateTime LastLogout { get; set; } = DateTime.MinValue;
        public int DeathCount { get; set; } = 0;
        public bool IsOnline { get; set; } = false;
    }
}
