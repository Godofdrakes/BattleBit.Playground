using BBRAPIModules;
using Commands;

namespace Playground.Respawn;

[RequireModule(typeof(CommandHandler))]
public class RespawnModule : BattleBitModule
{
	[CommandCallback("respawn")]
	public void RespawnCommand(RunnerPlayer player)
	{
		player.Kill();
	}
}