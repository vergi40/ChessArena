var PROTO_PATH = __dirname + '/../../Server/gRPC/protos/GameManager.proto';

// var async = require('async');
// var fs = require('fs');
// var parseArgs = require('minimist');
// var path = require('path');
// var _ = require('lodash');
var grpc = require('grpc');
var protoLoader = require('@grpc/proto-loader');
var packageDefinition = protoLoader.loadSync(
    PROTO_PATH,
    {keepCase: true,
     longs: String,
     enums: String,
     defaults: true,
     oneofs: true
    });
var protoDescription = grpc.loadPackageDefinition(packageDefinition);//namespace
var manager = protoDescription.GameManager;//namespace
var stub = new manager.WebService('localhost:30052', grpc.credentials.createInsecure());

/**
 * For testing purposes
 * @param {function} callback 
 */
function TestInitialize(){
  // var next = _.after(1, callback);
  // next();

  var request = {message: "ping"};
  stub.Ping(request, function(err, response) {
    if (err) {
      // process error
      console.log("Process error: " + err);
      return;
    } 
    else {
      // process feature
      if(response.message == "pong"){
        console.log("Received ping response from GameManager.")
      }
      else{
        console.log("Received faulty response from GameManager: " + response)
        return;
      }
    }
  });

  console.log("Starting to listen move updates")
  var moveIndex = 0;
  var call = stub.ListenMoveUpdates(request);

  // Invoked every time new move appears
  call.on('data', function(move) {
      console.log(moveIndex + " - Move received: " + move.chess);
      console.log("Move " + move.chess.start_position + " to " + move.chess.end_position)
      moveIndex++;
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
    console.log("Stream status updated to: " + status);
  });


}

TestInitialize();

// function main() {
//   async.series([
//     TestInitialize
//   ]);
// }

// if (require.main === module) {
//   main();
// }

// exports.Initialize = TestInitialize;