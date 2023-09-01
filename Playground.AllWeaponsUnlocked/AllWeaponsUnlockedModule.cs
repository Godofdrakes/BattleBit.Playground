using System.Threading.Tasks;
using BBRAPIModules;

namespace Playground.AllWeaponsUnlocked;

public class AllWeaponsUnlockedModule : BattleBitModule
{
	public override Task OnConnected()
	{
		Server.ServerSettings.UnlockAllAttachments = true;
		return Task.CompletedTask;
	}
}