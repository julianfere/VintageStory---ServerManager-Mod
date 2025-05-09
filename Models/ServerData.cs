﻿using System;
using System.Collections.Generic;

namespace ServerManager.Models
{
    public enum ServerState
    {
        Paused = 0,
        Ticking = 1
    }

    public class ServerData
    {
        private static ServerData _instance;
        private static readonly object _lock = new();

        public ServerState State;
        public int UpTimeInSeconds;
        public int TotalPlayTimeInSeconds;
        public Dictionary<string, PlayerData> Players;
        public WorldData WorldData;

        public ServerData()
        {
            Players = new Dictionary<string, PlayerData>();
            State = new ServerState();
            WorldData = new WorldData();
            UpTimeInSeconds = 0;
            TotalPlayTimeInSeconds = 0;
        }

        public void Update(ServerData data)
        {
            Players = data.Players;
            State = data.State;
            UpTimeInSeconds = data.UpTimeInSeconds;
            TotalPlayTimeInSeconds = data.TotalPlayTimeInSeconds;
            WorldData = data.WorldData;
            UpTimeInSeconds = data.UpTimeInSeconds;
            TotalPlayTimeInSeconds = data.TotalPlayTimeInSeconds;
        }

        public static ServerData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ServerData();
                    }
                }
                return _instance;
            }
        }
    }

}
