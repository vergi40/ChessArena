using vergiBlue.BoardModel;

namespace vergiBlue.Algorithms
{
    public static class CheckMate
    {
        public static bool InTwoTurns(IBoard boardAfterPlayerMove, bool isWhitePlayer)
        {
            // TODO could modify to support n depth
            // Opponent moves
            var opponentMoves = boardAfterPlayerMove.Moves(!isWhitePlayer, false);
            foreach (var opponentMove in opponentMoves)
            {
                var newBoard = BoardFactory.CreateFromMove(boardAfterPlayerMove, opponentMove);
                // Player moves
                var playerMoves = newBoard.Moves(isWhitePlayer, false);
                foreach (var playerMove in playerMoves)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, playerMove);
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
