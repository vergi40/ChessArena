# Chess arena
Developing own chess AI's from scratch to battle each other. Games are hosted on a local server. AI's communicate with game server over HTML/2 with [gRCP](https://grpc.io/) framework. Games are hosted to web front for watching


## Clients
* Free to use any language according to own preferences.
* Only requirement is to implement common interface [GameManager.proto](Server/gRPC/protos/GameManager.proto) to support all game server requirements. Supported gRPC languages: https://grpc.io/docs/languages/

### lipi WIP

### [vergiBlue C#](Clients/vergiBlue)
* Basic minimax-algorithm as base logic
* Includes [test server](Clients/vergiBlue/TestServer/) and console app where AI can play against itself
	* Full demo with two independent AI's hosted on TestServer over gRPC connection: [startlocalservergame.bat](Clients/vergiBlue/startlocalservergame.bat). Script builds all projects and starts new console for each app.
	* Graphical chess demo: Open solution and run "vergiBlueDesktop" project. Desktop chess has some settings, normal black or white start and some test game situations.
	* ![image](https://user-images.githubusercontent.com/16613890/138566824-393fe1c0-8c0b-46e9-b3ea-437e76d23a3e.png)
* Iterating more intelligence by teaching how to handle [various game situations through tests](Clients/vergiBlue/vergiBlueTests/)
* Minor goal is to include all connection-specific implementation in [Common-project](Clients/vergiBlue/CommonNetStandard/). 
	* C# clients should only need to implement shared abstract [LogicBase](Clients/vergiBlue/CommonNetStandard/Client/LogicBase.cs).
	* Example code how to implement your own ai in C# can be found in [Example.cs](Clients/vergiBlue/CommonNetStandard/Example.cs).


## Server
* Run locally on Raspberry Pi
* Python
* WIP


## Web
* Lightweight demo done in Node + React.js
* Found in [Web](Web)
* Using https://github.com/shaack/cm-chessboard for grpahics
* chess.js library for moves
* https://github.com/m-misha93/chess-react/blob/master/src/App.js as react example
* WIP


## Rough roadmap
* [Roadmap.md](./Roadmap.md)
