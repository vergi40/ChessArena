# TODO list

## Concepts
* IO-support 
  - FEN parsing http://www.fam-petzke.de/cp_fen_en.shtml
* Create benchmarking project
  - Perft tests. Multiple test positions, calculated to various depths. Track:
    - https://stackoverflow.com/questions/1110439/chess-optimizations
    - http://www.rocechess.ch/perft.html
    - https://www.chessprogramming.org/Perft
    - Time the search took
    - Amount of nodes searched
  - Bratko-Kopec positions 
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
* Search extensions
  - In certain cases, extend search depth or otherwise widen search
  - http://www.frayn.net/beowulf/theory.html#history
* En passant moves missing
* Aspiration window
* Quiescent search
* Null moves
* Monte Carlo search

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
* TDD move validation
  * Invalid move throws InvalidMoveException
  * This can be catched in main logic. Return backup move if stable game is required.

## Design
* Event system for turn start/end

