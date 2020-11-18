var PROTO_PATH = __dirname + '/../../Server/gRPC/protos/GameManager.proto';

var async = require('async');
var fs = require('fs');
var parseArgs = require('minimist');
var path = require('path');
var _ = require('lodash');
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
var manager = grpc.loadPackageDefinition(packageDefinition).GameManager;//namespace
var client = new manager.GameService('192.168.0.11:50051',
                                       grpc.credentials.createInsecure());


/**
 * For testing purposes
 * @param {function} callback 
 */
function Initialize(callback) {
  var next = _.after(1, callback);
    next();
  
  var gameInformation = {
    name: "node backend client",
    chess: ""
  };

  var startInfo = client.Initialize(gameInformation, function(err, feature){
    if (err) {
      // process error
      console.log("Node: Received error" )
    } else {
      // process feature
    }
  });
  console.log("Is starting player: " + startInfo.start)
}

function main() {
  async.series([
    Initialize
  ]);
}

if (require.main === module) {
  main();
}

exports.Initialize = Initialize;