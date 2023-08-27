# Infection

A pretty simple implementation of an Infection game mode. Basically [the official example](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki/Example:-Infected) but implemented as an [BattleBitAPIRunner](https://github.com/BattleBit-Community-Servers/BattleBitAPIRunner/) module.

## Installation

```json
{
  "IP": "127.0.0.1",
  "Port": 29294,
  "Modules": [
    "C:/Path/To/Project/BattleBit.Playground/Playground.Infection/InfectionModule.cs",
    "C:/Path/To/Project/BattleBit.Playground/Playground.Permissions/PermissionsModule.cs",
    "C:/Path/To/Project/BattleBit.Playground/Commands/CommandHandler.cs"
  ],
  "DependencyPath": "C:/Path/To/Project/BattleBit.Playground/Playground.Common/bin/Debug/net6.0"
}
```

`CommandHandler` and `PermissionsModule` are optional requirements.

You can also just paste the compiled DLL for `BattleBit.Playground/Playground.Common` into your existing dependency folder.

## Config

| Name                          | Type  | Default | Description                                                                                     |
|-------------------------------|-------|---------|-------------------------------------------------------------------------------------------------|
| InfectionPct                  | int   | 5       | What percentage of the total player pool should be infected?                                    |
| InfectionCheck                | int   | 30      | How often (in seconds) should the game check that there are enough infected players?            |
| InfectionRespawn              | int   | 10      | How many seconds does it take for infected players to respawn?                                  |
| InfectRunningSpeedMultiplier  | float | 2       | RunningSpeedMultiplier for infected players                                                     |
| InfectJumpHeightMultiplier    | float | 2       | JumpHeightMultiplier for infected players                                                       |
| InfectFallDamageMultiplier    | float | 0       | FallDamageMultiplier for infected players                                                       |
| InfectReceiveDamageMultiplier | float | 0.5     | ReceiveDamageMultiplier for infected players                                                    |
| InfectGiveDamageMultiplier    | float | 4       | GiveDamageMultiplier for infected players                                                       |
| CuredLives                    | int   | 1       | How many lives to cured players start with? Players that die with zero lives join the infected. |
| CuredRespawn                  | int   | 30      | How many seconds does it take for cured players to respawn?                                     |

## Commands

| Command       | Args        | Required Permission           | Description                             |
|---------------|-------------|-------------------------------|-----------------------------------------|
| curePlayer    | N/A         | Module.Infection.CureSelf     | Cure the player that runs the command   |
| infectPlayer  | N/A         | Module.Infection.InfectSelf   | Infect the player that runs the command |
| infectPlayers | (int) count | Module.Infection.InfectOthers | Select N random players to infect       |

If `PermissionsModule` is not loaded permission requirements will be ignored. All players will have access to all commands.
