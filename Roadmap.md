# Roadmap
Separate programming concerns to smaller pieces. Gradually improve features like ai, manager, connections and ui. 

## v0.1
- Chess ai
  - Valid moves for each piece
  - Decision logic can be random
  - Runs on Raspberry Pi 3(?)
- Connection inside system
  - Plain text document with simple file system watcher
- Connection interface
  - Ai: Join game [ai name]
  - Ai: Move [start coordinate, end coordinate, optional capture coordinate]
  - Manager: Game start [white player]
  - Manager: Invoke turn end [player]
- Ui
  - Most basic 8x8 with picture locations update based on given coordinates

## v0.2
- Chess ai
  - Minmax algorithm
  - Basic evaluation of board situation
- Connection inside system
  - [gRPC](https://grpc.io/docs/languages/) implementation
- Connection interface
  - Ai: Move additional data [diagnostic log]
  - WIP
- Ui
  - WIP

