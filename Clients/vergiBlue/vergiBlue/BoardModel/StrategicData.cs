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
    }
}