using System;
using ServerManager.Logger;
using ServerManager.Models;
using ServerManager.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerManager.Listener
{
    public class ServerEventListener : IServerEventListener
    {
        private readonly ICoreServerAPI _serverApi;
        private readonly ServerLogger _logger;
        private readonly JsonDataManager<ServerData> _store;
        private readonly ServerData _serverData;
        public ServerEventListener(ICoreServerAPI serverApi, ServerLogger logger, JsonDataManager<ServerData> store)
        {
            _serverApi = serverApi;
            _logger = logger;
            _store = store;
            _serverData = ServerData.Instance;
        }
        public void OnPlayerDeath(IServerPlayer player, DamageSource damageSource)
        {
            if (_serverData.Players.TryGetValue(player.PlayerUID, out PlayerData value))
            {
                value.DeathCount++;
            }
            else
            {
                _serverData.Players.Add(player.PlayerUID, new PlayerData
                {
                    PlayerUID = player.PlayerUID,
                    Name = player.PlayerName,
                    LastLogin = DateTime.Now,
                    DeathCount = 1
                });
            }

            try
            {
                _store.Update(oldData =>
                {
                    oldData.Players = _serverData.Players;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update player data in store: " + ex.Message);
            }
        }

        public void OnPlayerDisconnect(IServerPlayer player)
        {
            if (_serverData.Players.TryGetValue(player.PlayerUID, out PlayerData value))
            {
                value.LastLogout = DateTime.Now;
                value.IsOnline = false;
            }
            else
            {
                _serverData.Players.Add(player.PlayerUID, new PlayerData
                {
                    PlayerUID = player.PlayerUID,
                    Name = player.PlayerName,
                    LastLogout = DateTime.Now,
                    DeathCount = 1,
                    IsOnline = false
                });
            }

            try
            {
                _store.Update(oldData =>
                {
                    oldData.Players = _serverData.Players;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update player data in store: " + ex.Message);
            }
        }

        public void OnPlayerJoin(IServerPlayer player)
        {
            if (_serverData.Players.TryGetValue(player.PlayerUID, out PlayerData value))
            {
                value.LastLogin = DateTime.Now;
                value.IsOnline = true;
            }
            else
            {
                _serverData.Players.Add(player.PlayerUID, new PlayerData
                {
                    PlayerUID = player.PlayerUID,
                    Name = player.PlayerName,
                    LastLogin = DateTime.Now,
                    DeathCount = 1,
                    IsOnline = true
                });
            }

            try
            {
                _store.Update(oldData =>
                {
                    oldData.Players = _serverData.Players;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update player data in store: " + ex.Message);
            }
        }

        public EnumSuspendState OnServerResume()
        {
            _serverData.State = ServerState.Ticking;

            try
            {
                _store.Update(oldData =>
                {
                    oldData.State = ServerState.Ticking;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update server state in store: " + ex.Message);

            }
            return EnumSuspendState.Ready;
        }

        public void OnServerSuspend()
        {
            _serverData.State = ServerState.Paused;

            try
            {
                _store.Update(oldData =>
                {
                    oldData.State = ServerState.Paused;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update server state in store: " + ex.Message);
            }
        }

        public void OnGameWorldSaved()
        {
            try
            {
                _store.Update(oldData =>
                {
                    oldData.WorldData.LastSaved = DateTime.Now;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update server time in store: " + ex.Message);
            }
        }
    }
}
