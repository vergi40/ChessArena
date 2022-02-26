using vergiBlue.BoardModel;

namespace vergiBlue.Algorithms
{
    public static class CheckMate
    {
        public static bool InTwoTurns(IBoard boardAfterPlayerMove, bool isWhitePlayer)
        {
            // TODO should be deleted and normal minimax logic improved to handle
            // TODO could modify to support n depth
            // Opponent moves
                // TODO do castling need to be evaluated?
            var opponentMoves = boardAfterPlayerMove.MoveGenerator.MovesQuick(!isWhitePlayer, false);
            foreach (var opponentMove in opponentMoves)
            {
                var newBoard = BoardFactory.CreateFromMove(boardAfterPlayerMove, opponentMove);
                // Player moves
                // TODO do castling need to be evaluated?
                var playerMoves = newBoard.MoveGenerator.MovesQuick(isWhitePlayer, false);
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
