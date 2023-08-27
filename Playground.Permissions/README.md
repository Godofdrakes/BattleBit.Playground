# Privileges

A WIP module to add per-player permissions to a server.

## Installation

```json
{
  "IP": "127.0.0.1",
  "Port": 29294,
  "Modules": [
    "C:/Path/To/Project/BattleBit.Playground/Playground.Permissions/PermissionsModule.cs"
  ],
  "DependencyPath": "C:/Path/To/Project/BattleBit.Playground/Playground.Common/bin/Debug/net6.0"
}
```

You can also just paste the compiled DLL for `BattleBit.Playground/Playground.Common` into your existing dependency
folder.

## Config

Configuring `PermissionModule` requires you define all the available roles for your server (seperate
from `BattleBitAPI.Common.Roles`).

| Name        | Type         | Description                                          |
|-------------|--------------|------------------------------------------------------|
| Name        | string       | The unique name for this role                        |
| Description | string       | A user-friendly description of this role             |
| Permissions | List<string> | A list of all the permissions available to this role |

You must then declare which players are assigned what roles. You can assign multiple roles to a player, giving them all
the permissions within all of those roles. If a player does not have any roles assigned to them they have no
permissions.

| Name     | Type         | Description                           |
|----------|--------------|---------------------------------------|
| PlayerId | ulong        | The player's SteamId                  |
| Roles    | List<string> | All the roles assigned to this player |

An example config for this module would look like this.

```json
{
  "Roles": [
    {
      "Name": "Member",
      "Permissions": [
        "Module.Infection.CanInfectSelf"
      ]
    },
    {
      "Name": "Moderator",
      "Permissions": [
        "Module.Infection.CanInfectOthers"
      ]
    },
    {
      "Name": "Admin",
      "Permissions": [
        "Module.Infection.CanCureSelf"
      ]
    }
  ],
  "Players": [
    {
      "PlayerId": 117,
      "Roles": [
        "Member",
        "Moderator",
        "Admin"
      ]
    },
    {
      "PlayerId": 42,
      "Roles": [
        "Member"
      ]
    }
  ]
}
```
