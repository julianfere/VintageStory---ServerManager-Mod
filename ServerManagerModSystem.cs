using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("ServerManager", "servermanager",
                    Authors = new string[] { "julianfere_" },
                    Description = "Server manager console",
                    Version = "1.0.0")]

namespace ServerManager
{
    public class ServerManagerModSystem : ModSystem
    {
        private ServerManager? _serverManager;

        public override void StartServerSide(ICoreServerAPI api)
        {
            _serverManager = new ServerManager(this,api);
        }

        public override void Dispose()
        {
            _serverManager?.Dispose();
            base.Dispose();
        }
    }
}
