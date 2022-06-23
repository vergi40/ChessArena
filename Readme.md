# Chess arena
Developing own chess engines from scratch to battle each other. Games are hosted on a local server. Engines communicate with game server over HTML/2 with [gRCP](https://grpc.io/) framework. Games are hosted to web front for watching


## Clients (chess engines)
* Free to use any language according to own preferences.
* Only requirement is to implement common interface [GameManager.proto](Server/gRPC/protos/GameManager.proto) to support all game server requirements. Supported gRPC languages: https://grpc.io/docs/languages/


### Engine [vergiBlue C#](Clients/vergiBlue) (Teemu)
[![dev](https://github.com/vergi40/ChessArena/actions/workflows/vergiblue-build-and-test.yaml/badge.svg?branch=dev)](https://github.com/vergi40/ChessArena/actions)  
* General chess engine algorithms implemented in C#. Goal is to be easy to read and easy to maintain and upgrade. Main features:
	* Minimax
	* Alpha-beta pruning
	* Iterative deepening
	* Transposition tables
	* Graphical interface for playing and testing
	* ![image](https://user-images.githubusercontent.com/16613890/138566824-393fe1c0-8c0b-46e9-b3ea-437e76d23a3e.png)
* As the engine needs to calculate millions of boards per second, every small optimization can speed up game greatly. [BenchMarkDotnet](https://github.com/dotnet/BenchmarkDotNet) is used to test various operations to decide best design (e.g. 2D board array vs 1D, List or Array or Dictionary). Custom optimizations:
	* Board represented in 1D array
	* All slider attacks (rook, bishop, queen) for each square calculated beforehand
	* All pieces initialized for each square beforehand
	* Allocate move lists on stack instead of heap memory
	* Arrays instead of Dictionaries everywhere possible

#### Try out
* Includes [test server](Clients/vergiBlue/TestServer/) and console app where AI can play against itself
	* Full demo with two independent AI's hosted on TestServer over gRPC connection: [startlocalservergame.bat](Clients/vergiBlue/startlocalservergame.bat). Script builds all projects and starts new console for each app.
	* Graphical chess demo: Open solution and run "vergiBlueDesktop" project. Desktop chess has some settings, normal black or white start and some test game situations.


### Engine Lipponen WIP


## Server
* Original idea: run locally on Raspberry Pi with python
* Other ideas: run on cloud services
* Engines could also implement UCI protocol so games can be hosted in existing chess environments
* Work in progress


## Web
* Lightweight demo done in Node + React.js
* Found in [Web](Web)
* Using https://github.com/shaack/cm-chessboard for grpahics
* chess.js library for moves
* https://github.com/m-misha93/chess-react/blob/master/src/App.js as react example
* Work in progress

