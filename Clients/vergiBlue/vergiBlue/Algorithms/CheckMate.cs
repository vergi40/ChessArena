using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    public static class CheckMate
    {
        public static bool InTwoTurns(Board board, bool isWhitePlayer)
        {
            // TODO could modify to support n depth
            // Opponent moves
            var opponentMoves = board.Moves(!isWhitePlayer);
            foreach (var opponentMove in opponentMoves)
            {
                var newBoard = new Board(board, opponentMove);
                // Player moves
                var playerMoves = newBoard.Moves(isWhitePlayer);
                foreach (var playerMove in playerMoves)
                {
                    var nextBoard = new Board(newBoard, playerMove);
                    if (nextBoard.IsCheckMate(isWhitePlayer, false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
