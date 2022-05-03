# AoE2Lib

A .NET library for programmatically interacting with Age of Empires 2. The main goal is to provide an easy to use interface for .NET languages on top of FLWL's aoe2-ai-module (https://github.com/FLWL/aoe2-ai-module) and opening up Age of Empires 2 for bot programming and AI research similarly to the older Starcraft API.

## Features

The `AoEInstance` class is the main way of interacting with an AoE process. It works with both AoC and DE and provides several features.

### Bot interface

A bot is created by deriving from the abstract `Bot` class and calling `AoEInstance.StartBot`. See the `Unary` project for an example.

### Automatic game running

Construct an instance of the `Game` class and call `AoEInstance.StartGame`. This function is not available for DE.

### Dll injection

Provides an easy way to inject dll's into the AoE process by calling `AoEInstance.InjectDll`. It works across architectures, so you can inject a 32bit dll into a 32bit AoE process from a 64bit executable or vice versa. This makes it easy to support both AoC, which is 32bit, and DE, which is 64bit, from a single application.

## Included dependencies

aoc-auto-game: https://github.com/FLWL/aoc-auto-game

aoe2-ai-module: https://github.com/FLWL/aoe2-ai-module

FASM: http://flatassembler.net/
