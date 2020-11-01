namespace vergiBlue.Algorithms
{
    public static class CheckMate
    {
        public static bool InTwoTurns(Board board, bool isWhitePlayer)
        {
            // TODO could modify to support n depth
            // Opponent moves
            var opponentMoves = board.Moves(!isWhitePlayer, false);
            foreach (var opponentMove in opponentMoves)
            {
                var newBoard = new Board(board, opponentMove);
                // Player moves
                var playerMoves = newBoard.Moves(isWhitePlayer, false);
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
