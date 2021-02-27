# TODO list

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

## Techniques
* En passant moves missing
* Aspiration window
* Quiescent search
* Null moves
* Monte Carlo search