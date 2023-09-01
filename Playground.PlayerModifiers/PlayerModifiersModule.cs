using System.Threading.Tasks;
using BBRAPIModules;
using Commands;

namespace Playground.PlayerModifiers;

public class PlayerModifiersConfig : ModuleConfiguration
{
	public float RunningSpeedMultiplier { get; set; } = 1;
	public float ReceiveDamageMultiplier { get; set; } = 1;
	public float GiveDamageMultiplier { get; set; } = 1;
	public float JumpHeightMultiplier { get; set; } = 1;
	public float FallDamageMultiplier { get; set; } = 1;
	public float ReloadSpeedMultiplier { get; set; } = 1;
}

public class PlayerModifiersModule : BattleBitModule
{
	public PlayerModifiersConfig Config { get; set; } = new();

	[ModuleReference]
	public CommandHandler? CommandHandler { get; set; }

	public override void OnModulesLoaded()
	{
		CommandHandler?.Register(this);
	}

	public override Task OnPlayerSpawned(RunnerPlayer player)
	{
		player.Modifications.RunningSpeedMultiplier = Config.RunningSpeedMultiplier;
		player.Modifications.ReceiveDamageMultiplier = Config.ReceiveDamageMultiplier;
		player.Modifications.GiveDamageMultiplier = Config.GiveDamageMultiplier;
		player.Modifications.JumpHeightMultiplier = Config.JumpHeightMultiplier;
		player.Modifications.FallDamageMultiplier = Config.FallDamageMultiplier;
		player.Modifications.ReloadSpeedMultiplier = Config.ReloadSpeedMultiplier;
		return Task.CompletedTask;
	}

	[CommandCallback("playerModifiers.load")]
	public void Load() => Config.Save();

	[CommandCallback("playerModifiers.save")]
	public void Save() => Config.Save();
}