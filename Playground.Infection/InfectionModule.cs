﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BBRAPIModules;
using BBRAPIModules.Commands;
using JetBrains.Annotations;

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
	public int CuredRespawn { get; set; } = 30;
}

[UsedImplicitly]
[RequireModule(typeof(CommandHandler))]
public class InfectionModule : BattleBitModule
{
	private const Team TEAM_CURED = Team.TeamA;
	private const Team TEAM_INFECTED = Team.TeamB;

	[UsedImplicitly]
	public InfectionModuleConfig Config { get; set; } = new();

	[UsedImplicitly]
	public CommandHandler? CommandHandler { get; set; }

	public IEnumerable<RunnerPlayer> CuredPlayers => Server.AllTeamAPlayers;
	public IEnumerable<RunnerPlayer> InfectedPlayers => Server.AllTeamBPlayers;

	public int InfectedPlayerCount => InfectedPlayers.Count();
	public int InfectedPlayerCountTarget => Convert
		.ToInt32(Server.CurrentPlayerCount * (float) Config.InfectionPct / 100);

	private DateTime _infectionCheckLast = DateTime.MaxValue;
	private Action<RunnerPlayer>? _playerConnected;
	private Action<RunnerPlayer>? _playerDied;
	private Func<RunnerPlayer, Team, bool>? _playerRequestingToChangeTeam;

	private readonly Dictionary<ulong, int> _playerLives = new();

	public InfectionModule()
	{
		SetGameState(GameState.WaitingForPlayers);
	}

	public override void OnModulesLoaded()
	{
		CommandHandler?.Register(this);
	}

	public override Task OnTick()
	{
		// todo: don't add code to tick. use a task scheduler and pump it here.

		var infectionCheck = Math.Max(1, Config.InfectionCheck);
		var elapsed = DateTime.Now - _infectionCheckLast;
		if (elapsed >= TimeSpan.FromSeconds(infectionCheck))
		{
			var infectedCount = InfectedPlayerCount;
			var infectedTarget = InfectedPlayerCountTarget;

			// ensure enough players are infected
			if (infectedCount < infectedTarget)
			{
				InfectRandomPlayers(infectedTarget - infectedCount);
			}
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
		player.ChangeTeam(TEAM_CURED);

		if (bAnnounce)
			Server.AnnounceShort($"{player.Name} has been cured!");

		_playerLives[player.SteamID] = Math.Max(1, Config.CuredLives);
	}

	[CommandCallback("infectPlayers")]
	private void InfectRandomPlayers(int count, bool bAnnounce = true)
	{
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
		player.ChangeTeam(TEAM_INFECTED);

		if (bAnnounce)
			Server.AnnounceShort($"{player.Name} has been infected!");

		_playerLives[player.SteamID] = 0;
	}

	[CommandCallback("curePlayer")]
	public void CurePlayerCommand(RunnerPlayer player) { CurePlayer(player, true); }

	[CommandCallback("infectPlayer")]
	private void InfectPlayerCommand(RunnerPlayer player) { InfectPlayer(player, true); }

	[CommandCallback("infectPlayers")]
	private void InfectPlayersCommand(int count) { InfectRandomPlayers(count, true); }

	private static void Swap<T>(IList<T> items, int first, int second)
	{
		var temp = items[first];
		items[second] = items[first];
		items[first] = temp;
	}

	private static bool IsCured(RunnerPlayer player) => player.Team == TEAM_CURED;
	private static bool IsInfected(RunnerPlayer player) => player.Team == TEAM_INFECTED;
}