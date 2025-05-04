using System;

namespace ServerManager.Models
{
    public class WorldData
    {
        public string Name { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public int Day { get; set; } = 0;
        public string Time { get; set; } = string.Empty;
        public DateTime LastSaved { get; set; } = DateTime.MinValue;
        public DateTime LastBackup { get; set; } = DateTime.MinValue;
    }
}
