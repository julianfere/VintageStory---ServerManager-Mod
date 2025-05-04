using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerManager.command;
public class CommandHandler
{
    private readonly IChatCommand _chatCommand;
    private readonly ServerManager _server;

    public CommandHandler(ServerManager server)
    {
        _server = server;

        _chatCommand = server.ServerAPI.ChatCommands
        .Create(server.ModId)
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(_ => TextCommandResult.Success("funca bien loco"));
    }

    public void RegisterSubCommand(AbstractCommand command)
    {
        _chatCommand
            .BeginSubCommands(command.Name)
            .WithDescription(command.Description)
            .WithArgs(command.ArgParsers)
            .HandleWith(args =>
            {
                if (!args.Caller.HasPrivilege(command.Privilege))
                {
                    return TextCommandResult.Error("no tene permiso");
                }
                if (command.RequiresPlayer && args.Caller.Player == null)
                {
                    return TextCommandResult.Error("Este lo podes correr solo si sos jugador");
                }
                try
                {
                    return command.Execute(args);
                }
                catch (Exception e)
                {
                    return TextCommandResult.Error(e.Message);
                }
            })
            .EndSubCommand();
    }
}

