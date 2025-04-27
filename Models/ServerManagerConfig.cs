using System.IO;
using System;

namespace ServerManager.Models
{
    public class ServerManagerConfig
    {
        public string DataPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
    }
}
