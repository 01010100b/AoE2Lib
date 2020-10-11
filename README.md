# AoE2Lib

A .NET standard 2.0 library for programmatically interacting with Age of Empires 2. Check the [releases](https://github.com/01010100b/AoE2Lib/releases) page to download the latest release.

## Features

- ### Reading and writing the goal and sn arrays of players

What we need is to construct a `GameInstance` object by giving it an AoE2 process to connect to. At this time only WK is supported.
~~~~
Process process = Process.GetProcessesByName("WK")[0];
GameInstance instance = new WKInstance(process);
~~~~
We can then access the goal and sn arrays for any player through this `instance` object by calling the following methods on it.
~~~~
int[] GetGoals(int player);
bool SetGoal(int player, int index, int value);
bool SetGoals(int player, int start_index, params int[] values);

int[] GetStrategicNumbers(int player);
bool SetStrategicNumber(int player, int index, int value);
bool SetStrategicNumbers(int player, int start_index, params int[] values);
~~~~
These methods will return `null` or `false` if there is no game running or the given player is not in the game. The `GameInstance` class also has some utility functions for this.
~~~~
bool HasExited
bool IsGameRunning
bool IsPlayerInGame(int player)
~~~~
Note that read/write access to the goal/sn arrays is not synchronized in any way.

- ### Having an easy object-oriented API for AI development

This is currently under development. See the `Bot` class if you want an idea on how it looks. This will be further developed alongside the Quaternary bot, so you can also take a look at that project to see how the `Bot` interface would work.

- ### Having an easy object-oriented API for loading .dat mod data

This will probably be a layer on top of AocDatLib.