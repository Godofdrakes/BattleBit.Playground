using BBRAPIModules;
using Commands;

namespace Playground.Respawn;

[RequireModule(typeof(CommandHandler))]
public class RespawnModule : BattleBitModule
{
	public CommandHandler? CommandHandler { get; set; }

	public override void OnModulesLoaded()
	{
		CommandHandler?.Register(this);
	}

	[CommandCallback("respawn")]
	public void RespawnCommand(RunnerPlayer player)
	{
		player.Kill();
	}
}