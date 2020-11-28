# Chess arena
Developing own chess AI's from scratch to battle each other. Games are hosted on a local server. AI's communicate with game server over HTML/2 with [gRCP](https://grpc.io/) framework. Games are hosted to web front for watching


## Clients
* Free to use any language according to own preferences.
* Only requirement is to implement common interface [ChessArena.proto](Server/ChessArena.proto) to make connection to game server possible. Supported gRPC languages: https://grpc.io/docs/languages/

### lipi WIP

### [vergiBlue C#](Clients/vergiBlue)
* Basic minimax-algorithm as base logic
* Iterating more intelligence by teaching how to handle [various game situations through tests](Clients/vergiBlue/vergiBlueTests/)
* Includes [test server](Clients/vergiBlue/TestServer/) and console app where AI can play against itself
* Minor goal is to include all connection-specific implementation in [Common-project](Clients/vergiBlue/CommonNetStandard/). 
	* C# clients should only need to implement shared abstract [LogicBase](Clients/vergiBlue/CommonNetStandard/Client/LogicBase.cs).
	* Example code how to implement your own ai in C# can be found in [Example.cs](Clients/vergiBlue/CommonNetStandard/Example.cs).


## Server
* Run locally on Raspberry Pi
* Python
* WIP


## Web
* WIP


## Rough roadmap
* [Roadmap.md](./Roadmap.md)
