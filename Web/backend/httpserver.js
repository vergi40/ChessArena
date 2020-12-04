const express = require("express");
const bodyparser = require("body-parser");
const cors = require("cors");

const app = express();

// app.get('/', (req, res) => {
//     res.send("GET command to root");
// });

app.use(bodyparser.urlencoded({extended: false}));
app.use(bodyparser.json());
app.use(cors());

// Use static files
// app.use(express.static(__dirname + "/public"));

// app.get('*', (req, res) => {
//     console.log("GET request received");
//     // console.log(req.query);//http://localhost:3000/?name=teemu
//     // console.log(req.body);// all body data (in post)
//     // console.log(req.header);// all header data in json
//     // console.log(req.params);// calling get/:id with 1234 
//     res.send("backend respond")
//     // res.sendFile(path.join(__dirname, 'dist/index.html'));
//     // res.status(404).send("not found");
// });

app.get("/test1", (req, res) => {
  console.log("GET /test1 request received");
  // Start test session 1
  res.send("backend got your test1 http get")
  // res.json({message: "backend got your test1 http get"})
})

app.listen(3000);