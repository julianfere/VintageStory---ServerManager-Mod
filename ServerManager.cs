using System;
using ServerManager.Listener;
using ServerManager.Logger;
using ServerManager.Models;
using ServerManager.Server;
using ServerManager.Utils;
using Vintagestory.API.Server;

namespace ServerManager
{
    public class ServerManager
    {
        private readonly ICoreServerAPI _serverApi;
        private readonly JsonDataManager<ServerData> _jsonDataManager;
        private readonly ServerLogger _logger;
        private readonly WebServer _webServer;
        private readonly ServerEventListener _listener;
        private readonly ServerManagerModSystem _modSystem;
        public ICoreServerAPI ServerAPI => _serverApi;
        public string ModId => _modSystem.Mod.Info.ModID;

        internal const string ConfigFile = "ServerManager.json";
        internal static ServerManagerConfig Config { get; set; } = null!;
        private long _pullServerDataListenerId;

        public ServerManager(ServerManagerModSystem mod,ICoreServerAPI api)
        {
            _serverApi = api;
            _logger = new ServerLogger(api.Logger);

            try
            {
                Config = _serverApi.LoadModConfig<ServerManagerConfig>(ConfigFile);
            }
            catch (Exception e)
            {
                _logger.LogError("Error loading config: " + e.Message);
            }

            Config ??= new ServerManagerConfig();
            _jsonDataManager = new JsonDataManager<ServerData>(Config.DataPath, "serverdata.json");

            _webServer = new WebServer(this,_logger, _jsonDataManager);

            try
            {
                _webServer.StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Error starting web server: " + e.Message);
            }

            ServerEventListener _listener = new(_serverApi, _logger, _jsonDataManager);

            try
            {
                _serverApi.Event.PlayerDisconnect += _listener.OnPlayerDisconnect;
                _serverApi.Event.PlayerJoin += _listener.OnPlayerJoin;
                _serverApi.Event.PlayerDeath += _listener.OnPlayerDeath;
                _serverApi.Event.ServerResume += () => _listener.OnServerResume();
                _serverApi.Event.GameWorldSave += _listener.OnGameWorldSaved;
                _serverApi.Event.ServerSuspend += () =>
                {
                    _listener.OnServerSuspend();
                    return EnumSuspendState.Ready;
                };
                _pullServerDataListenerId = _serverApi.Event.RegisterGameTickListener(PullServerData, 3000);
            }
            catch (Exception e)
            {
                _logger.LogError("Error registering events: " + e.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                _serverApi.Event.PlayerDisconnect -= _listener.OnPlayerDisconnect;
                _serverApi.Event.PlayerJoin -= _listener.OnPlayerJoin;
                _serverApi.Event.PlayerDeath -= _listener.OnPlayerDeath;
                _serverApi.Event.ServerResume -= () => _listener.OnServerResume();
                _serverApi.Event.ServerSuspend -= () =>
                {
                    _listener.OnServerSuspend();
                    return EnumSuspendState.Ready;
                };
                _serverApi.Event.GameWorldSave -= _listener.OnGameWorldSaved;
                _serverApi.Event.UnregisterGameTickListener(_pullServerDataListenerId);
                _webServer.StopAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Error unregistering events: " + e.Message);
            }
        }

        private void PullServerData(float _)
        {
            try
            {
                _jsonDataManager.Update(data =>
                {
                    string fdate = _serverApi.World.Calendar.PrettyDate();
                    string time = fdate.Split(" ")[4];
                    data.WorldData.Day = Int32.Parse(fdate.Split(".")[0]);
                    data.WorldData.Year = _serverApi.World.Calendar.Year;
                    data.WorldData.Month = MonthMapper.MapMonthEnumToString(_serverApi.World.Calendar.MonthName);
                    data.WorldData.Time = time;
                    data.UpTimeInSeconds = _serverApi.Server.TotalWorldPlayTime;
                    data.TotalPlayTimeInSeconds = _serverApi.Server.ServerUptimeSeconds;
                    data.WorldData.Name = _serverApi.WorldManager.SaveGame.WorldName;
                    data.Update(data);
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Error updating server data: " + e.Message);
            }
        }
    }
}
