using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BBRAPIModules;
using Playground.Common;

namespace Playground.Moderation;

public class ModerationConfig : ModuleConfiguration
{
	public Dictionary<ulong, string> BanList { get; set; } = new();
	public Dictionary<ulong, string> MuteList { get; set; } = new();
}

public static class ModerationPermissions
{
	public const string MUTE = "Module.Moderation.Mute";
	public const string KICK = "Module.Moderation.Kick";
	public const string BAN = "Module.Moderation.Ban";
}

public class ModerationModule : BattleBitModule
{
	public ModerationConfig Config { get; set; } = new();

	private readonly TextWriter _logger;

	public ModerationModule()
	{
		_logger = Console.Out;
	}

	public void KickPlayer(ulong playerId, string reason)
	{
		_logger.WriteLine($"Kicking player: {playerId}");

		Server.Kick(playerId, $"Kicked for: {reason}");
	}

	public void BanPlayer(ulong playerId, string reason)
	{
		_logger.WriteLine($"Banning player: {playerId}");
		
		Server.Kick(playerId, $"Banned for: {reason}");

		lock (Config)
		{
			Config.BanList[playerId] = reason;
			Config.Save();
		}
	}

	public void UnbanPlayer(ulong playerId)
	{
		_logger.WriteLine($"Unbanning player: {playerId}");

		lock (Config)
		{
			if (Config.BanList.Remove(playerId))
			{
				Config.Save();
			}
		}
	}

	public void MutePlayer(ulong playerId, string reason)
	{
		_logger.WriteLine($"Muting player: {playerId}");

		lock (Config)
		{
			Config.MuteList[playerId] = reason;
			Config.Save();
		}
	}

	public void UnmutePlayer(ulong playerId)
	{
		_logger.WriteLine($"Unmuting player: {playerId}");

		lock (Config)
		{
			if (Config.MuteList.Remove(playerId))
			{
				Config.Save();
			}
		}
	}
	
	public override Task OnPlayerJoiningToServer(ulong steamId, PlayerJoiningArguments args)
	{
		string? banReason;

		lock (Config) Config.BanList.TryGetValue(steamId, out banReason);

		if (banReason is not null)
		{
			_logger.WriteLine($"Blocked player. SteamId: {steamId}");
			Server.Kick(steamId, $"Banned for: {banReason}");
		}

		return Task.CompletedTask;
	}

	public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string msg)
	{
		string? muteReason;

		lock (Config) Config.MuteList.TryGetValue(player.SteamID, out muteReason);

		if (muteReason is not null)
		{
			return Task.FromResult(false);
		}

		return Task.FromResult(true);
	}

	[Command("kick", requiredPermission: ModerationPermissions.KICK)]
	private void KickCommand(RunnerPlayer contextPlayer, RunnerPlayer targetPlayer, string reason)
	{
		KickPlayer(targetPlayer.SteamID, reason);
	}

	[Command("mute", requiredPermission: ModerationPermissions.MUTE)]
	private void MuteCommand(RunnerPlayer contextPlayer, RunnerPlayer targetPlayer, string reason)
	{
		MutePlayer(targetPlayer.SteamID, reason);
	}

	[Command("unmute", requiredPermission: ModerationPermissions.MUTE)]
	private void MuteCommand(RunnerPlayer contextPlayer, RunnerPlayer targetPlayer)
	{
		UnmutePlayer(targetPlayer.SteamID);
	}

	[Command("ban", requiredPermission: ModerationPermissions.BAN)]
	private void BanCommand(RunnerPlayer contextPlayer, RunnerPlayer targetPlayer, string reason)
	{
		BanPlayer(targetPlayer.SteamID, reason);
	}

	[Command("unban", requiredPermission: ModerationPermissions.BAN)]
	private void BanCommand(RunnerPlayer contextPlayer, RunnerPlayer targetPlayer)
	{
		UnbanPlayer(targetPlayer.SteamID);
	}
}