# "Ostrovsky A"s Source Code - Skillz 2016
In this repository you will find the source code, as it was at the beginning of the final's day, used by the group "Ostrovsky A" in the "Skillz 2016" competition.

Note: We nicknamed ourself "PoodleChanBD", so you might see some references to this name throughout the code.

## General Structure
### Wrapping
We completely wrapped the objects given to us by the game's API with our own objects, which were updated automatically every turn. The wrapping allowed us to save the objects in other classes without the worry of updating them, and made it easier for us the add custom attributes and methods to them (for example, we calculated each turn which pirate carries which treasure, which is something that is not available through the normal API). We were worried about the wrapping before the finals, because we knew that we needed to add support for the new feature in our API, which takes time. As a result, we added instructions on the addition and changing needed for general objects that could be added, and added "hack ins" to retrieve the original objects from ours.

### Code Structure
This year, the main idea of our bot was to use what we called "events". Each event knows how to handle a local situation in-game. For example, the situation of a pirate carrying a treasure: The event ReturnTreasureEvent knows how to detect a pirate which is currently carrying a treasure, and sends him home.

Each turn, after the update of the wrappers, each event is being "asked" by the main method which actions we should do: the actions are being proposed by what we call an "ActionsPack"; each ActionsPack contains a list of "commands" we want to execute (For example, attack, defend, sail and so forth), and it has a numerical value, which indicates "how good" is it– The higher the better.

After all the events returned their desired ActionsPacks and their values, we choose the **best combination** of ActionsPacks, such that the ActionsPacks do not "overlap", meaning, they do not use the same resources (For example, you can't execute two ActionsPacks that use the same pirate). Ideally, the best combination is being chosen and then executed each turn, but when the time limit approaches, the code will exit early and will be satisfied with a "good enough" combination.

## Code Description by Directories
The main directory contains some general files:
- MyBot.cs: The main class of the bot; actually control all the execution of the code.
- KeyGeneration.py: A python script used to generate encryption and decryption keys (for C# and JavaScript, accordingly), to be used when encrypting the debug messages.
- zipper.py: A python script used to quickly zip the C# source code, in all the relevant directories.

### Actions
This directory contains the infrastructure for creating and choosing ActionsPacks. As the problem of the packs' choosing is NP-Complete (See [0-1 linear integer programming]( https://en.wikipedia.org/wiki/Integer_programming)), we had to use complex heuristics in order to speed up the choosing process.

#### Actions\Commands
This directory contains the commands that can be executed by the game. The commands' mechanic automatically disables the option of trying to execute impossible / undesirable commands, thus we needed special commands for special cases (For example, RamCommand for ramming our own pirate, and DoNothingCommand for doing nothing, which is normally undesirable).

#### Actions\Resources
This directory contains the classes used for the choosing heuristics, and ResourcesPack.cs, which is the main class the makes sure no two commands/packs use the same resources. ResourcesPacks support the addition of "Imaginary Resources" that no two packs can share, and thus we can easily make sure no two packs that were proposed by the same event will be executed together, which is sometimes a desirable behavior.

##### Actions\Resources\Overlappers
This directory contains "Overlappers", which are generic classes that are used in the heuristics process.

### API
This directory contains all the wrapping of the original game's objects. The files in this directory are the files that are supposed to be reachable by classes from other directories, for example events. The class Game.cs, encapsulates all the methods we implemented throughout the sub-directories, to make the use of the methods much more clear, easier and accessible throughout the code, which doesn’t need to know which method comes from which class.

#### API\Debug
This directory contains the Logger, which is a centralized location where all the debug messages go through. As a result, the logger contains easy "gates", which make it possible to quickly turn on and off certain types of logging messages, and turn on and off the complete encryption of the debug messages.

#### API\General
This directory contains the Turn object, which contains general information about the current turn. In addition, the Turn keeps track of how much time do we have until the turn ends; during the competition we found bugs in the original game's function, and thus we had to manually fix it on our side. Also, we found out that the time given to each turn is less than what the game actually says, so we had to introduce a "glass ceiling", which makes sure **we never get close to the end of the turn**, in order to prevent timeouts at all costs.

#### API\Mapping
This directory contains the following classes:
- AttackMap.cs: This class is used to exchange between speed and space, by precalculating which pirate can attack which location on the map.
- Map.cs: This class has some utility methods that can be applied on objects that have a location in-game, and also contains the other classes in this directory. The class also manages the treasures by TreasureStates (free to take, currently being carried by a pirate and taken).
- PirateManager.cs: This class manages the pirates of one side (in this case, ours or the enemy's). The class manages the pirates by PirateStates (free, currently carrying treasure, drunk and lost).
- SailManager.cs: This class manages the movement of pirates. The class has two main methods:
   - GetNaiveSailOptions: Returns the sail options, using our wrapping objects, as they were returned from the original GetSailOptions method. We use this method in order to predict movement of enemy pirates.
   - GetCompleteSailOptions: This method, along with all its overloads, controls the main movement of our pirates in game, using a lot of parameters (really. *A lot*). Originally the class used an algorithm called [A*](http://theory.stanford.edu/~amitp/GameProgramming/AStarComparison.html) , that uses pathfinding to calculate the actual distance from the starting point to the goal point, while considering custom obstacles (such as treasures locations, enemy locations, dangerous locations and so on). As the code grew and the time given to each turn became shorter, we understood that the algorithm isn't fast enough for us, and when, during a game, we realize that we're using too much time,  we go back to calculate distance using normal [Manhattan Distance]( https://en.wikipedia.org/wiki/Taxicab_geometry).

#### API\Prediction
This directory contains a generic infrastructure for predicting things in-game. The PredictionManager uses a lot of simple PiratePredictions, which are being asked each turn what they think will happen next turn; the next turn the manager checks which predictions were true and which were wrong, and thus each PiratePrediction gets a score of how accurate it usually is. The prediction available through the manager is the one he gets from the PiratePrediction with the highest recorded accuracy. 

##### API\Prediction\Attack
This directory contains the possible PiratePredictions the bot use to try and predict whether the enemy will attack next turn.

##### API\Prediction\Movement
This directory contains the possible PiratePredictions the bot use to try and predict where each pirate will be next turn.

### Events
This directory contains the Events of the bot, "the strategic mind and brain of the bot". All the strategic calculations are done in this directory. You can see each file as a possible "strategy" that the bot might want to use in order to win on a local scale.

### States
This directory contains the "States" of the bot: a State is an object which is reachable by all the events of the bot, which saves a general state of the game. For example, it saves whether we / the enemy are close to winning, whether a treasure is "threatened" by some defined standards and so on.

### Utilities
This directory contains some generic utilities classes and methods, which are being used throughout the project.

## And always remember... :)
<img src="https://www.ezphotoshare.com/images/2016/03/31/7zdcb.gif" width="300" />
