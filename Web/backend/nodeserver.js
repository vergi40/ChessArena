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
var stub = new manager.WebService('localhost:50052', grpc.credentials.createInsecure());

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
    } 
    else {
      // process feature
      if(response == "pong"){
        console.log("Received ping response from GameManager.")
      }
      else{
        console.log("Received faulty response from GameManager: " + response)
      }
    }
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