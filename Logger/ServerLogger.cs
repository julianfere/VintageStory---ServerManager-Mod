using ServerManager.Listener;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerManager.Logger
{
    public class ServerLogger
    {
        private readonly ILogger _logger;

        public ServerLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void OnPlayerJoin(IServerPlayer player)
        {
            LogPlayerEvent("Join", player);
        }

        public void OnPlayerDisconnect(IServerPlayer player)
        {
            LogPlayerEvent("Disconnect", player);
        }

        public void OnPlayerDeath(IServerPlayer player, DamageSource damageSource)
        {
            LogPlayerEvent("Death", player);
        }

        public void OnServerSuspend()
        {
            LogServerEvent("Server Suspended");
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public EnumSuspendState OnServerResume()
        {
            LogServerEvent("Server Resumed");
            
            return EnumSuspendState.Ready;
        }

        private void LogPlayerEvent(string eventType, IServerPlayer player)
        {
            _logger.Notification("==========================================================================================");
            _logger.Notification($"{eventType} - Player: {player.PlayerName}, {player.PlayerUID}");
            _logger.Notification("==========================================================================================");
        }

        private void LogServerEvent(string message)
        {
            _logger.Notification("==========================================================================================");
            _logger.Notification(message);
            _logger.Notification("==========================================================================================");
        }
    }
}
