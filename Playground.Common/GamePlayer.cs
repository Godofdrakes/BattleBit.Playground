using BattleBitAPI;

namespace Playground.Common;

public class GamePlayer : Player<GamePlayer>
{
	private readonly CancellationTokenSource _cts = new();

	public event Action<GamePlayer>? Connected;

	public override Task OnConnected()
	{
		Connected?.Invoke(this);
		return Task.CompletedTask;
	}
}