import './App.css';
import "./styles/cm-chessboard.css"
import React, {Component} from 'react';
import Chess from "chess.js";
import {Chessboard, MOVE_INPUT_MODE, COLOR} from "cm-chessboard"

// For styling
import 'tachyons';

// Using https://github.com/shaack/cm-chessboard for grpahics
// chess.js for moves
// https://github.com/m-misha93/chess-react/blob/master/src/App.js as react example


class App extends Component {
  constructor() {
    super();
    this.state = {
      fen: 'start',
      history: [],
    };
  }

  // Initialize
  componentDidMount() {
    console.log("Component App mounted to DOM.")
    this.game = new Chess();
    this.board = new Chessboard(document.getElementById("board"), {
      position: this.game.fen(),
      orientation: COLOR.white,
      moveInputMode: MOVE_INPUT_MODE.viewOnly,
      // moveInputMode: MOVE_INPUT_MODE.dragPiece,
      responsive: true,
      sprite: {
        // url: "../assets/images/chessboard-sprite.svg", // pieces and markers are stored as svg in the sprite
        url: "./chessboard-sprite.svg", // pieces and markers are stored as svg in the sprite
        grid: 40 // the sprite is tiled with one piece every 40px
    }
    });

    // this.board.enableMoveInput(this.inputHandler);
  }

  // User can create moves by dragging
  // Waiting for use case

  // inputHandler = (event) => {
  //   if (event.type === INPUT_EVENT_TYPE.moveDone) {

  //     const move = this.game.move({
  //       from: event.squareFrom,
  //       to: event.squareTo,
  //       promotion: 'q',
  //     });

  //     if (move === null) return;

  //     this.setState(() => ({
  //       fen: this.game.fen(),
  //       history: this.game.history({verbose: true}),
  //     }));

  //     setTimeout(() => {
  //       event.chessboard.setPosition(this.game.fen())
  //     })
  //   }
  // };

  onButtonTest1 = () => {
    // GET
    fetch("http://localhost:3000/test1")
    // .then(res => res.json())
    .then(res => res.text())
    .then(data => {

      // Add timestamp
      var now = new Date();
      // convert date to a string in UTC timezone format:
      console.log(now.toLocaleTimeString());
      // Output: Wed, 21 Jun 2017 09:13:01 GMT
      
      console.log(now + " response received: " + data);
      this.game.move({from: "a2", to: "a3"});
      this.fen = this.game.fen();
      this.board.setPosition(this.fen);

    })
  }

  // Frontpage render
  render() {

    return (
      <div className="tc">
        <h1 className="f1">ChessArena web experience</h1>
        <div className="flex justify-center">
          <p className="outline w-25 pa3 mr2">placeholder</p>
          <div
            id='board'
            className="outline w-30 ma3"
          >
          </div>
          <p className="outline w-25 pa3 mr2">placeholder</p>
        </div>
        <div className="form pa4 br3 shadow-2">
					<button 
						className="w-8 pa3 mr2 "
						onClick={this.onButtonTest1}> Test 1
					</button>
				</div>
      </div>

    );
  }
}

export default App;
