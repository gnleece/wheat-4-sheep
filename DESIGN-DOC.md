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

Basic features that are not implemented yet:
* Longest road
* End game / victory state
* Trading

## Advanced game features

Once all the core game functionality is implemented, some ideas for advanced features:
* Networked multiplayer
* Intelligent AI (with adjustable difficulty level)

## Known bugs and improvements

* All of the UI is ugly placeholder UI
* Selecting a location for road building is hard, the hitbox is too small
