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
            foreach (var opponentMove in board.Moves(!isWhitePlayer))
            {
                var newBoard = new Board(board, opponentMove);
                // Player moves
                foreach (var playerMove in newBoard.Moves(isWhitePlayer))
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
