import './App.css';
import Board from "./Board.js";

function App() {
  // Note: Consider implementing https://github.com/shaack/cm-chessboard

  // Here we draw the Board react-component
    return (
    <div className="tc">
      <h1 className="f1">ChessArena web experience</h1>
      <Board />
    </div>
  );
}

export default App;
