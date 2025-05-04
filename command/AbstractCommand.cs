using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerManager.command;

public abstract class AbstractCommand
{
    protected readonly IServerAPI _server;

    public string[] Name { get; }
    public string Description { get; }
    public string Privilege { get; }
    public readonly bool RequiresPlayer;
    public readonly ICommandArgumentParser[] ArgParsers;

    protected AbstractCommand(IServerAPI server, string[] name, string privilege = null, bool requiresPlayer = false, params ICommandArgumentParser[] argParsers)
    {
        _server = server;

        Name = name;
        Description = "";
        Privilege = privilege ?? Vintagestory.API.Server.Privilege.root;
        RequiresPlayer = requiresPlayer;
        ArgParsers = argParsers;
    }

    public abstract TextCommandResult Execute(TextCommandCallingArgs args);
}