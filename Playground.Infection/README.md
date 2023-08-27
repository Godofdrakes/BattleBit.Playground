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

| Command       | Args        | Description                             |
|---------------|-------------|-----------------------------------------|
| curePlayer    | N/A         | Cure the player that runs the command   |
| infectPlayer  | N/A         | Infect the player that runs the command |
| infectPlayers | (int) count | Select N random players to infect       |

Commands currently have no permission requirements and can be run by anyone
