using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ServerManager.Listener;
using ServerManager.Utils;
using ServerManager.Models;
using System;
using ServerManager.Logger;
using ServerManager.Server;

[assembly: ModInfo("ServerManager", "servermanager",
                    Authors = new string[] { "julianfere_" },
                    Description = "Server manager console",
                    Version = "1.0.0")]

namespace ServerManager
{
    public class ServerManagerModSystem : ModSystem
    {
        private ICoreServerAPI _serverApi;
        private JsonDataManager<ServerData> _jsonDataManager;
        private ServerLogger _logger;
        private WebServer _webServer;

        internal const string ConfigFile = "ServerManager.json";
        internal static ServerManagerConfig Config { get; set; } = null!;
        public override void StartServerSide(ICoreServerAPI api)
        {
            _serverApi = api;
            _logger = new ServerLogger(Mod.Logger);
            _webServer = new WebServer(_logger);

            try
            {
                _webServer.StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("Error starting web server: " + e.Message);
            }

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
            ServerEventListener listener = new(_serverApi, _logger, _jsonDataManager);

            try
            {
                _serverApi.Event.PlayerDisconnect += listener.OnPlayerDisconnect;
                _serverApi.Event.PlayerJoin += listener.OnPlayerJoin;
                _serverApi.Event.PlayerDeath += listener.OnPlayerDeath;
                _serverApi.Event.ServerResume += () => listener.OnServerResume();
                _serverApi.Event.ServerSuspend += () =>
                {
                    listener.OnServerSuspend();
                    return EnumSuspendState.Ready;
                };
                _serverApi.Event.RegisterGameTickListener(PullServerData, 5000);
            }
            catch (Exception e)
            {
                _logger.LogError("Error registering events: " + e.Message);
            }
        }

        public override void Dispose()
        {
            _webServer?.StopAsync();
            base.Dispose();
        }

        private void PullServerData(float _)
        {
            try
            {
                _jsonDataManager.Update(data =>
                {
                    data.WorldData.Day = _serverApi.World.Calendar.DayOfYear / 12 / 9;
                    data.WorldData.Year = _serverApi.World.Calendar.Year;
                    data.WorldData.Month = MonthMapper.MapMonthEnumToString(_serverApi.World.Calendar.MonthName);
                    float floatHour = _serverApi.World.Calendar.HourOfDay;
                    int hour = (int)floatHour;
                    int minutes = (int)((floatHour - hour) * 60);
                    data.WorldData.Time = $"{hour}:{minutes}hs";
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Error updating server data: " + e.Message);
            }
        }
    }
}
