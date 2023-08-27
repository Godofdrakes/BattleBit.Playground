using BBRAPIModules;

namespace Playground.Common;

public interface ICommandsModule
{
	void Register(BattleBitModule module);
}