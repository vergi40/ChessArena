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
      inspectionMode: false
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
      this.setState ({fen: this.game.fen()});
      this.board.setPosition(this.fen);
    })
  }

  onButtonResetState = () => {
    this.game = new Chess();
    this.board.setPosition(this.game.fen());
    this.setState ({
      fen: this.game.fen(),
      history: [],
      inspectionMode: false
    });
  }

  onButtonLoadPGN = () => {
    console.log("Not implemented yet")

    const pgn = [
      '[Event "Casual Game"]',
      '[Site "Berlin GER"]',
      '[Date "1852.??.??"]',
      '[EventDate "?"]',
      '[Round "?"]',
      '[Result "1-0"]',
      '[White "Adolf Anderssen"]',
      '[Black "Jean Dufresne"]',
      '[ECO "C52"]',
      '[WhiteElo "?"]',
      '[BlackElo "?"]',
      '[PlyCount "47"]',
      '',
      '1.e4 e5 2.Nf3 Nc6 3.Bc4 Bc5 4.b4 Bxb4 5.c3 Ba5 6.d4 exd4 7.O-O',
      'd3 8.Qb3 Qf6 9.e5 Qg6 10.Re1 Nge7 11.Ba3 b5 12.Qxb5 Rb8 13.Qa4',
      'Bb6 14.Nbd2 Bb7 15.Ne4 Qf5 16.Bxd3 Qh5 17.Nf6+ gxf6 18.exf6',
      'Rg8 19.Rad1 Qxf3 20.Rxe7+ Nxe7 21.Qxd7+ Kxd7 22.Bf5+ Ke8',
      '23.Bd7+ Kf8 24.Bxe7# 1-0'
    ]
  
    this.game.load_pgn(pgn.join('\n'))
    console.log(this.game.history({verbose: true}))
    this.board.setPosition(this.game.fen());

    // Jump to last position
    this.setState ({
      fen: this.game.fen(),
      history: this.game.history({verbose: true}),
      inspectionMode: true
    });
  }

  onButtonLoadFEN = () => {
    console.log("Not implemented yet")
    
  }

  onButtonDebug = () => {
    console.log("debug 1")
    console.log("inspectionMode: " + this.state.inspectionMode);
    this.setState({inspectionMode: true});
    console.log("inspectionMode: " + this.state.inspectionMode);

  }

  onButtonStepBack = () => {
    console.log("Not implemented yet")
    
  }

  onButtonStepForward = () => {
    console.log("Not implemented yet")
    
  }

  onButtonStepStart = () => {
    console.log("Not implemented yet")
    
  }

  onButtonStepEnd = () => {
    console.log("Not implemented yet")
    
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
        <button 
						className="w-8 pa3 mr2 "
						onClick={this.onButtonTest1}> Start streaming
				</button>
        <button 
						className="w-8 pa3 mr2 "
						onClick={this.onButtonResetState}> Reset state
				</button>
        <button 
						className="w-8 pa3 mr2 "
						onClick={this.onButtonLoadPGN}> Load full PGN game
				</button>
        <button 
						className="w-8 pa3 mr2 "
						onClick={this.onButtonDebug}> Debug tests
				</button>
        {this.state.inspectionMode ? <ControlButtons 
          stepStart = {this.onButtonStepStart} 
          stepBack = {this.onButtonStepBack} 
          stepForward = {this.onButtonStepForward} 
          stepEnd = {this.onButtonStepEnd} /> : null}        
      </div>

    );
  }
}

// Game history controls
class ControlButtons extends Component {
  constructor (props) {
    super(props);
  }

  render(){
    return(
      <div className="control-buttons pa3 mr2">
        <button
          className="w-8 pa3 mr2 "
          onClick={this.props.stepStart}> Go to start
        </button>
        <button
          className="w-8 pa3 mr2 "
          onClick={this.props.stepBack}> Step back
        </button>
        <button
          className="w-8 pa3 mr2 "
          onClick={this.props.stepForward}> Step forward
        </button>
        <button
          className="w-8 pa3 mr2 "
          onClick={this.props.stepEnd}> Go to end
        </button>
      </div>);
  }
}

export default App;
