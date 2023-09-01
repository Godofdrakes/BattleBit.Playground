using System.Threading.Tasks;
using BBRAPIModules;

namespace Playground.MOTD;

public class MOTDConfig : ModuleConfiguration
{
	public string Message { get; set; } = string.Empty;
}

public class MOTDModule : BattleBitModule
{
	public MOTDConfig Config { get; set; } = new();

	public override Task OnPlayerConnected(RunnerPlayer player)
	{
		if (!string.IsNullOrEmpty(Config.Message))
		{
			player.Message(Config.Message);
		}

		return Task.CompletedTask;
	}
}