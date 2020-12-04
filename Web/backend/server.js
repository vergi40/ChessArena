// General definitions
//
// Node doesn't fully support ES6 features (like import/export)
// Add to package.json:   "type": "module",
// import {SharedData} from "./sharedData.js";
const hostingIp = "localhost";
const grpcPort = 30052;
const webSocketsServerPort = 8000;

// Grpc imports
//
const PROTO_PATH = __dirname + '/../../Server/gRPC/protos/GameManager.proto';
const grpc = require('grpc');
const protoLoader = require('@grpc/proto-loader');
const packageDefinition = protoLoader.loadSync(
    PROTO_PATH,
    {keepCase: true,
     longs: String,
     enums: String,
     defaults: true,
     oneofs: true
    });
const protoDescription = grpc.loadPackageDefinition(packageDefinition);//namespace
const manager = protoDescription.GameManager;//namespace
const stub = new manager.WebService(hostingIp + ":" + grpcPort , grpc.credentials.createInsecure());

// Websockets imports
//
const webSocketServer = require('websocket').server;
const http = require('http');

// General variables
//
const clients = {};

class SharedData{
  movesReceived = 0;
  movesSent = 0;
  moveHistory = [];

  getNextMoveInJSON() {
      // 
      let data = this.moveHistory[this.movesSent];
      let json = JSON.stringify({
          from: data.start_position, 
          to: data.end_position,
          promotion: data.promotion_result,
          diagnostics: data.diagnostics
      });
      return json;
  }
}
const sharedData = new SharedData();

// Start servers
//
const server = http.createServer();
server.listen(webSocketsServerPort);
const wsServer = new webSocketServer({
  httpServer: server
});

console.log("1");

// Start service cycle
// 
// var counter = 0;
// var pingResult = false;
// while(pingResult === false){
//     setTimeout(() => {
//       console.log(counter + " - Trying to ping game manager.")
//       counter++;
//       pingResult = PingTest();
//       console.log("asd")
//     }, 
//     1000);
// }

console.log("Trying to ping game manager.")
var pingResult = PingTest(); // Should be synchronous but looks like not?
console.log(pingResult);
if(!pingResult) return;

console.log("2");

// Ping ok. Start listening gRPC moves. This is linked to sharedData
StartListeningMoves();

console.log("3");

// Set to start streaming to frontend when request is received
wsServer.on('request', function(request) {
    var userID = getUniqueID();
    console.log((new Date()) + ' Received a new connection from origin ' + request.origin + '.');
    // You can rewrite this part of the code to accept only the requests from allowed origin
    const connection = request.accept(null, request.origin);
    clients[userID] = connection;
    console.log('connected: ' + userID + ' in ' + Object.getOwnPropertyNames(clients))
    
    console.log("Sent: " + sharedData.movesSent + ", received: " + sharedData.movesReceived);

    // console.log("Debug history: " + JSON.stringify(sharedData.moveHistory, null, 2));
    // Start streaming
    while(true){
        // setTimeout(() => {
        //   if(sharedData.movesSent < sharedData.movesReceived){
        //     // There is a new move available to be sent to frontend
        //     let move = sharedData.getNextMoveInJSON();
        //     connection.send(move);
        //     sharedData.movesSent++;
        //     console.log("Sent move to frontend: " + move);
        //   }
        // }, 2000);  
        
        if(sharedData.movesSent < sharedData.movesReceived){
          // There is a new move available to be sent to frontend
          let move = sharedData.getNextMoveInJSON();
          connection.send(move);
          sharedData.movesSent++;
          console.log("Sent move to frontend: " + JSON.stringify(move, null, 2));
        }
    }
});

function sleep(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
} 

function PingTest(){
    let request = {message: "ping"};
    let pingResult = stub.Ping(request, function(err, response) {
      if (err) {
        // process error
        console.log("Process error: " + err);
        return false;
      } 
      else {
        // process feature
        if(response.message == "pong"){
          console.log("Received ping response from GameManager.");
          return true;
        }
        else{
          console.log("Received faulty response from GameManager: " + response)
          return false;
        }
      }
    });

    return pingResult;
}  

function StartListeningMoves(){
    console.log("Starting to listen move updates")
    var moveIndex = 0;
    let request = {message: "ping"};
    var call = stub.ListenMoveUpdates(request);

    // Invoked every time new move appears
    call.on('data', function(move) {
        // console.log(moveIndex + " - Move received: " + JSON.stringify(move, null, 2));
        console.log("Move " + move.chess.start_position + " to " + move.chess.end_position)
        moveIndex++;
        sharedData.movesReceived++;
        sharedData.moveHistory.push(move.chess);
        console.log("Move count:" + sharedData.moveHistory.length)
    });
    call.on('end', function() {
    console.log("Stream ended - game over");
    // The server has finished sending
    });
    call.on('error', function(e) {
    // An error has occurred and the stream has been closed.
    console.log("Error occured during stream: " + e);
    });
    call.on('status', function(status) {
    // process status
    console.log("Stream status updated to: " + JSON.stringify(status, null, 2));
    });
}


const getUniqueID = () => {
    const s4 = () => Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    return s4() + s4() + '-' + s4();
};
