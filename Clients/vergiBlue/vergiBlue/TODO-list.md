# TODO list

## Concepts
* Create benchmarking project
  - http://www.netlib.org/utk/lsi/pcwLSI/text/node354.html#SECTION001635000000000000000
  - Alter search class structure so more data can be recorded
* Evaluation overhaul
  - Evaluation strategy (start/mid game, endgame)
  - Pawn transpositions
  - King transpositions
  - Pawn structure
  - King safety
  - Center control
  - King attack
  - Rook bonuses
  - http://www.netlib.org/utk/lsi/pcwLSI/text/node343.html#SECTION001631100000000000000
Evaluation strategies
  - Inject strategy (instance) to move algorithm. Normal / endgame / check
* Search extensions
  - In certain cases, extend search depth or otherwise widen search
  - http://www.frayn.net/beowulf/theory.html#history
* En passant moves missing
* Aspiration window
* Quiescent search
* Null moves
* Monte Carlo search
* Attack squares
  - Indirect check
  - Bonus if opponent king in attack square
  - Castling rights
  - Legal moves


## Dev
* Create comment print from FEN
* Output FEN-string from sandbox


## User experience
* Better highlight for invalid squares
* Better highlight for last move
* Animation
* Click sound
* Difficulty levels
* Timer
* More highlights in game events, like checkmate
* More move information
* Move history

## Things the AI needs improvements
* If no captures available, does pretty dumb backing moves
* Stalemate should be avoided
* Transposition logic
  * Update checkmate value after fetching
  * https://www.ics.uci.edu/~eppstein/180a/970424.html
* Iterative move ordering
  * Use transposition results as approximations
  * Killer heuristics
* Castling not in transposition data
* En passant not in transposition data
* Attack squares
  * This is tricky, as calculating for each depth is really expensive
  * For main logic board, could contain 'current turn data'

## Design
* Event system for turn start/end

