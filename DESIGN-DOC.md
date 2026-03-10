# Wheat-4-Sheep Design Doc

This is a game based on Settlers of Catan, built in Unity with C#.

## Core game features

Basic features that are working:
* Support for 3 or 4 players (1 local human player and 2-3 AI players)
* Board layout is randomly generated at game start, and player can choose to regenerate it
* Initial settlement/road placement
* Basic turn flow with resource distribution
* Settlment, city, and road building
* Robber
* Development cards: all 5 types (Knight, Victory Point, Year of Plenty, Monopoly, Road Building) and Largest Army
* Trading: player-to-bank (4:1 default, 3:1 or 2:1 with ports) and player-to-player
* Ports (9 ports randomly placed on the board perimeter: four 3:1 generic and one 2:1 per resource type)

Basic features that are not implemented yet:
* Longest road
* UI that shows a log of all player actions, with colors and icons for easy readability

## Advanced game features

Once all the core game functionality is implemented, some ideas for advanced features:
* Networked multiplayer
* Intelligent AI (with adjustable difficulty level)

## Known bugs and improvements

### Small
* Selecting a location for road building is hard, the hitbox is too small
* Result of dice roll isn't diplayed to the user
* Result of development card purchase isn't displayed to the user
* Action button states flicker on/off while AI players are taking their turns (they should all stay greyed out unitl it's the local human player's turn)
* If the player buys 2 dev cards in a single turn and they're the same type (e.g. two knights), the game allows one of them to be played, which should not be possible. If Catan rules prevent purchasing multiple dev cards in one turn, then the fix should be to limit it to 1 purchase. If on the other hand the rules allow purchasing multiple per turn but none of them can be played in the same turn, then the logic for preventing a card from being played in the turn where it was purchased has a bug
* The colors used for each resource should match the colors used for their corresponding hex tiles
* Player can't cancel building a road/settlement/city once they enter that mode
* Sub-managers get initialized at inconsistent/arbitrary places

### Large
* All of the UI is ugly placeholder UI
