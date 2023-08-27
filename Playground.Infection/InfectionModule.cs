using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using Playground.Common;

namespace Playground.Infection;

public class InfectionModuleConfig : ModuleConfiguration
{
	public int InfectionPct { get; set; } = 5;
	public int InfectionCheck { get; set; } = 30;
	public int InfectionRespawn { get; set; } = 10;
	public float InfectRunningSpeedMultiplier { get; set; } = 2;
	public float InfectJumpHeightMultiplier { get; set; } = 2;
	public float InfectFallDamageMultiplier { get; set; } = 0;
	public float InfectReceiveDamageMultiplier { get; set; } = 0.5f;
	public float InfectGiveDamageMultiplier { get; set; } = 4;

	public int CuredLives { get; set; } = 1;
	public int CuredCheck { get; set; } = 1;
	public int CuredRespawn { get; set; } = 30;
}

[RequireModule(typeof(CommandHandler))]
public class InfectionModule : BattleBitModule
{
	private const Team TEAM_CURED = Team.TeamA;
	private const Team TEAM_INFECTED = Team.TeamB;

	private const string PERMISSION_INFECT_OTHERS = "Module.Infection.InfectOthers";
	private const string PERMISSION_INFECT_SELF = "Module.Infection.InfectSelf";
	private const string PERMISSION_CURE_SELF = "Module.Infection.CureSelf";

	public InfectionModuleConfig Config { get; set; } = new();

	public CommandHandler? CommandHandler { get; set; }

	[ModuleReference]
	public IPermissionsModule? PermissionsModule { get; set; }

	public IEnumerable<RunnerPlayer> CuredPlayers => Server.AllTeamAPlayers;
	public IEnumerable<RunnerPlayer> InfectedPlayers => Server.AllTeamBPlayers;

	public int InfectedPlayerCount => InfectedPlayers.Count();

	public int InfectedPlayerCountTarget => Convert
		.ToInt32(Server.CurrentPlayerCount * (float) Config.InfectionPct / 100);

	private DateTime _infectionCheckLast = DateTime.MaxValue;
	private DateTime _curedCheckLast = DateTime.MaxValue;

	private Action<RunnerPlayer>? _playerConnected;
	private Action<RunnerPlayer>? _playerDied;
	private Func<RunnerPlayer, Team, bool>? _playerRequestingToChangeTeam;

	private readonly Dictionary<ulong, int> _playerLives = new();

	private readonly TextWriter _logger;

	public InfectionModule()
	{
		_logger = Console.Out;

		SetGameState(GameState.WaitingForPlayers);
	}

	public override void OnModulesLoaded()
	{
		CommandHandler?.Register(this);

		Console.WriteLine($"PermissionsModule: {PermissionsModule?.GetType().Name ?? "null"}");
	}

	public override Task OnTick()
	{
		// todo: don't add code to tick. use a task scheduler and pump it here.
		var now = DateTime.Now;

		var infectionCheck = Math.Max(1, Config.InfectionCheck);
		var infectionCheckElapsed = DateTime.Now - _infectionCheckLast;
		if (infectionCheckElapsed >= TimeSpan.FromSeconds(infectionCheck))
		{
			var infectedCount = InfectedPlayerCount;
			var infectedTarget = InfectedPlayerCountTarget;

			_logger.WriteLine($"infected players: {infectedCount}, target: {infectedTarget}");

			// ensure enough players are infected
			if (infectedCount < infectedTarget)
			{
				InfectRandomPlayers(infectedTarget - infectedCount);
			}

			_infectionCheckLast = now;
		}

		var curedCheck = Math.Max(1, Config.CuredCheck);
		var curedCheckElapsed = DateTime.Now - _curedCheckLast;
		if (curedCheckElapsed >= TimeSpan.FromSeconds(curedCheck))
		{
			if (!CuredPlayers.Any())
			{
				Server.AnnounceLong("the infected devour the last survivor");
				Server.ForceEndGame();
			}

			_curedCheckLast = now;
		}

		return Task.CompletedTask;
	}

	public override Task OnPlayerConnected(RunnerPlayer player)
	{
		_playerConnected?.Invoke(player);
		return Task.CompletedTask;
	}

	public override Task<bool> OnPlayerRequestingToChangeTeam(RunnerPlayer player, Team requestedTeam)
	{
		var bAllowed = _playerRequestingToChangeTeam?.Invoke(player, requestedTeam) ?? true;
		return Task.FromResult(bAllowed);
	}

	public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(RunnerPlayer player, OnPlayerSpawnArguments request)
	{
		if (IsInfected(player))
		{
			request.Loadout = new PlayerLoadout
			{
				PrimaryWeapon = default,
				SecondaryWeapon = default,
				FirstAidName = string.Empty,
				LightGadgetName = string.Empty, // todo: could we put pickaxe here?
				HeavyGadgetName = Gadgets.SuicideC4.Name,
				ThrowableName = string.Empty,
			};

			request.SpawnProtection = 0;
		}

		return Task.FromResult<OnPlayerSpawnArguments?>(request);
	}

	public override Task OnPlayerSpawned(RunnerPlayer player)
	{
		if (IsInfected(player))
		{
			player.Modifications.RunningSpeedMultiplier = Config.InfectRunningSpeedMultiplier;
			player.Modifications.JumpHeightMultiplier = Config.InfectJumpHeightMultiplier;
			player.Modifications.FallDamageMultiplier = Config.InfectFallDamageMultiplier;
			player.Modifications.ReceiveDamageMultiplier = Config.InfectReceiveDamageMultiplier;
			player.Modifications.GiveDamageMultiplier = Config.InfectGiveDamageMultiplier;
			player.Modifications.RespawnTime = Config.InfectionRespawn;
		}

		if (IsCured(player))
		{
			player.Modifications.RespawnTime = Config.CuredRespawn;
		}

		return Task.CompletedTask;
	}

	public override Task OnPlayerDied(RunnerPlayer player)
	{
		_playerDied?.Invoke(player);
		return Task.CompletedTask;
	}

	private void SetGameState(GameState gameState)
	{
		_logger.WriteLine($"entering gameState: {gameState}");

		if (gameState >= GameState.Playing)
		{
			// start the timer
			_infectionCheckLast = DateTime.MinValue;

			// infect late-joining players
			_playerConnected = player => InfectPlayer(player, false);

			_playerDied = player =>
			{
				if (player.Team != TEAM_CURED) return;

				_playerLives.TryGetValue(player.SteamID, out var lives);

				if (lives <= 0)
				{
					InfectPlayer(player);
				}
				else
				{
					_playerLives[player.SteamID] = Math.Max(0, lives - 1);
				}
			};

			// block changing team to cured
			_playerRequestingToChangeTeam = (_, team) => team == TEAM_INFECTED;
		}
		else
		{
			_infectionCheckLast = DateTime.MaxValue;
			_playerConnected = player => CurePlayer(player);
			_playerRequestingToChangeTeam = (_, _) => true;
		}
	}

	public override Task OnGameStateChanged(GameState oldState, GameState newState)
	{
		SetGameState(newState);
		return Task.CompletedTask;
	}

	private void CurePlayer(RunnerPlayer player, bool bAnnounce = false)
	{
		_logger.WriteLine($"curing {player.Name}");

		player.ChangeTeam(TEAM_CURED);

		if (bAnnounce)
			Server.AnnounceShort($"{player.Name} has been cured!");

		_playerLives[player.SteamID] = Math.Max(1, Config.CuredLives);
	}

	private void InfectRandomPlayers(int count, bool bAnnounce = true)
	{
		_logger.WriteLine($"infecting {count} players");

		var players = CuredPlayers.ToArray();

		// randomize the player list
		for (var index = 0; index < players.Length - 1; ++index)
		{
			if (Random.Shared.Next() % 2 == 0)
			{
				Swap(players, index, players.Length - 1);
			}
		}

		// infect the first N players in the list
		foreach (var player in players[..count])
		{
			InfectPlayer(player, bAnnounce);
		}
	}

	private void InfectPlayer(RunnerPlayer player, bool bAnnounce = true)
	{
		_logger.WriteLine($"infecting {player.Name}");

		player.ChangeTeam(TEAM_INFECTED);

		if (bAnnounce)
			Server.AnnounceShort($"{player.Name} has been infected!");

		_playerLives[player.SteamID] = 0;
	}

	[CommandCallback("curePlayer")]
	public void CurePlayerCommand(RunnerPlayer player)
	{
		if (PermissionsModule.HasPermission(player, PERMISSION_CURE_SELF))
		{
			CurePlayer(player, true);
		}
	}

	[CommandCallback("infectPlayer")]
	private void InfectPlayerCommand(RunnerPlayer player)
	{
		if (PermissionsModule.HasPermission(player, PERMISSION_INFECT_SELF))
		{
			InfectPlayer(player, true);
		}
	}

	[CommandCallback("infectPlayers")]
	private void InfectPlayersCommand(RunnerPlayer player, int count)
	{
		if (PermissionsModule.HasPermission(player, PERMISSION_INFECT_OTHERS))
		{
			InfectRandomPlayers(count, true);
		}
	}

	private static void Swap<T>(IList<T> items, int first, int second)
	{
		var temp = items[first];
		items[second] = items[first];
		items[first] = temp;
	}

	private static bool IsCured(RunnerPlayer player) => player.Team == TEAM_CURED;
	private static bool IsInfected(RunnerPlayer player) => player.Team == TEAM_INFECTED;
}