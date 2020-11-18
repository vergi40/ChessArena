const express = require("express");
const bodyparser = require("body-parser");

const app = express();

app.get('/', (req, res) => {
    res.send("GET command to root");
});

app.listen(3000);