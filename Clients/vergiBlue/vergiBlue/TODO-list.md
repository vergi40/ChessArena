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
* Transposition logic. In tests seems like this doesn't improve search time
  * Castling not in transposition data
  * En passant not in transposition data

## Techniques
* En passant moves missing
* Aspiration window
* Quiescent search
* Null moves
* Monte Carlo search