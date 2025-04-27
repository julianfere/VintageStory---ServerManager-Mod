using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerManager.Listener
{
    interface IServerEventListener
    {
        void OnPlayerJoin(IServerPlayer player);
        void OnPlayerDisconnect(IServerPlayer player);
        void OnPlayerDeath(IServerPlayer player, DamageSource damageSource);
        void OnServerSuspend();
        EnumSuspendState OnServerResume();
    }
}
