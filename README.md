# AoE2Lib

A .NET standard 2.1 library for programmatically interacting with Age of Empires 2.

## Features

The `AoEInstance` class is the main way of interacting with an AoE process. It works with both AoC and DE and provides several features.

### Bot programming

See the classes in the `Bots` namespace. A bot is created by deriving from the abstract `Bot` class and calling `AoEInstance.StartBot`. When the game is finished you then have to call `Bot.Stop` yourself as there is, as of now, no way of determining when a game has ended. See the `ExampleBot` project.

### Game running

Construct an instance of the `Game` class and call `AoEInstance.StartGame`. This function is not available for DE. A .NET5 WinForms `GameRunner` application is included as well.

### Dll injection

Provides an easy way to inject dll's into the AoE process by calling `AoEInstance.InjectDll`. It works across architectures, so you can inject a 32bit dll into a 32bit AoE process from a 64bit executable or vice versa. This makes it easy to support both Aoc, which is 32bit, and DE, which is 64bit, from a single application.

## Included dependencies

aoc-auto-game: https://github.com/FLWL/aoc-auto-game

aoe2-ai-module: https://github.com/FLWL/aoe2-ai-module
