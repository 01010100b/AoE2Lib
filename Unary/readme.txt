Unary AI by 01010100
====================

For DE: The ai API module used by Unary may (about 50% of the time in my experience) trip the tampering detection or even just crash the game. 
Unary is hence not supported on DE, though there's nothing stopping you from trying.

Installation
------------

Unzip Unary anywhere you want except for the "Program Files" folder (it makes logging, and thereby Unary itself, extremely slow).
Copy Unary.ai and Unary.per to your AoE2 scripts folder. 

Playing a game
--------------

Perform the following steps in sequence:

1. Start Age of Empires 2.

2. Start Unary.

3. Enter the AoE2 process name and click "Connect to process". 
The process name is normally the filename of the game executable without the ".exe" part, usually "WK" for WK or "AoE2DE_s" for DE.

4. For each player you want controlled by Unary, enter the player number and click "Start for player".

5. Start the game in AoE2.
If everything works then each Unary player should chat an OK message in game around 10-20 seconds in.

6. When the game is finished, click "Stop all players".
If you want to start a new game, go back to step 4. 
You should "Stop all players" and go back to step 4 even if you want to use Unary again for the same player or even just restart the same game.

Troubleshooting
---------------

Several log files are created in Unary's folder. 
The main Unary application will write to "Unary.log" and each Unary player p will write to "Player p.log".
Please provide the relevant log files (preferably zipped, they can get quite large) when making a bug report.

Various
-------

The latest Unary release can be found here: https://github.com/01010100b/AoE2Lib/releases
The source code can be found here: https://github.com/01010100b/AoE2Lib
The author can also be found at
- The AI scripters forum: https://forums.aiscripters.com/index.php
- The AI scripters discord: https://discord.gg/sGWMgGtQp9
