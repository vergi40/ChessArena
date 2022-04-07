# TODO list

## Concepts
* Aspiration window search
* Principal Variation search
* Quiescent search
* Evaluation overhaul
  * Evaluation strategy (start/mid game, endgame)
  * Pawn transpositions
  * King transpositions
  * Pawn structure
  * King safety
  * Center control
  * King attack
  * Rook bonuses
  * http://www.netlib.org/utk/lsi/pcwLSI/text/node343.html#SECTION001631100000000000000
Evaluation strategies
  * Inject strategy (instance) to move algorithm. Normal / endgame / check
* Search extensions
  * In certain cases, extend search depth or otherwise widen search
  * http://www.frayn.net/beowulf/theory.html#history
* En passant moves missing
* Null moves
* Monte Carlo search
* Output FEN strings
* Implement Universal Chess Interface
  * https://home.hccnet.nl/h.g.muller/engine-intf.html
  * Add engine to battle other engines in lichess
  * https://github.com/ShailChoksi/lichess-bot 
  * Task-based methods
* Heuristics
  * Killer moves


## Dev
* Create comment print from FEN
* Fuller sandbox experience. Output FEN-string from sandbox
* Logging


## Testing
* Add long-running testing, where older engine versions are battled against latest one in various scenarios
* Add principal variation methods to output "best" moves to depth n


## User experience
* Better sandbox
* Better highlight for invalid squares
* Better highlight for last move
* Animation
* Click sound
* Difficulty levels
* Timer
* More highlights in game events, like checkmate
* More move information
* Move history
