using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    /// <summary>
    /// Data class that is updated for each depth in search.
    /// </summary>
    public class StrategicData
    {
        /// <summary>
        /// Tracks turn count for search.
        /// </summary>
        public int TurnCountInCurrentDepth { get; set; }
        
        /// <summary>
        /// How close to end the game is. Range 0.0 (start) - 1.0 (empty board).
        /// </summary>
        public double EndGameWeight { get; set; }

        /// <summary>
        /// Queen side
        /// </summary>
        public bool WhiteLeftCastlingValid { get; set; } = true;
        /// <summary>
        /// King side
        /// </summary>
        public bool WhiteRightCastlingValid { get; set; } = true;
        public bool BlackLeftCastlingValid { get; set; } = true;
        public bool BlackRightCastlingValid { get; set; } = true;

        /// <summary>
        /// For testing. Don't want to use opening book for arbitrary test situations. 
        /// </summary>
        public bool SkipOpeningChecks { get; set; } = false;

        /// <summary>
        /// If opponent pawn did a 2 square move, add tile behind it as en passant target. Otherwise null
        /// </summary>
        public (int column, int row)? EnPassantPossibility { get; set; } = null;

        public StrategicData()
        {
            // Empty board = end value-
            EndGameWeight = 1;
            TurnCountInCurrentDepth = 0;
        }

        public StrategicData(StrategicData previous)
        {
            EndGameWeight = previous.EndGameWeight;
            WhiteLeftCastlingValid = previous.WhiteLeftCastlingValid;
            WhiteRightCastlingValid = previous.WhiteRightCastlingValid;
            BlackLeftCastlingValid = previous.BlackLeftCastlingValid;
            BlackRightCastlingValid = previous.BlackRightCastlingValid;
            EnPassantPossibility = previous.EnPassantPossibility;
            SkipOpeningChecks = previous.SkipOpeningChecks;
        }


        public void UpdateCastlingStatusFromMove(SingleMove move)
        {
            if (move.NewPos.row == 0)
            {
                WhiteLeftCastlingValid = false;
                WhiteRightCastlingValid = false;
            }
            else if(move.NewPos.row == 7)
            {
                BlackLeftCastlingValid = false;
                BlackRightCastlingValid = false;
            }
        }

        /// <summary>
        /// Status from FEN string (e.g. KQkq)
        /// </summary>
        public void SetCastlingStatus(string status)
        {
            WhiteRightCastlingValid = status.Contains('K');
            WhiteLeftCastlingValid = status.Contains('Q');
            BlackRightCastlingValid = status.Contains('k');
            BlackLeftCastlingValid = status.Contains('q');
        }

        /// <summary>
        /// Update after each executed move
        /// </summary>
        public void UpdateEnPassantStatus(SingleMove move, PieceBase piece)
        {
            EnPassantPossibility = null;
            if (piece.Identity != 'P') return;
            if (piece.IsWhite)
            {
                if (move.PrevPos.row == 1 && move.NewPos.row == 3)
                {
                    EnPassantPossibility = (move.NewPos.column, 2);
                }
            }
            else
            {
                if (move.PrevPos.row == 6 && move.NewPos.row == 4)
                {
                    EnPassantPossibility = (move.NewPos.column, 5);
                }
            }
        }
    }
}