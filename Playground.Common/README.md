# Common

This is a test case for some additions to the BattleBitAPIRunner I'm working on. The goal is to support referencing
optional modules through strongly typed interfaces. The IPermissionsModule and ICommandsModule are examples of this.

There is no BattleBitModule in this library. Instead, the compiled DLL should be added to the runner's dependencies
folder. There it will be available for all modules to reference.