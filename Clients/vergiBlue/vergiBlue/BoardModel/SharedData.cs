using vergiBlue.Algorithms;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    /// <summary>
    /// Data class that has same informations shared to all search depths.
    /// </summary>
    public class SharedData
    {
        /// <summary>
        /// Total game turn count
        /// </summary>
        public int GameTurnCount { get; set; } = 0;

        /// <summary>
        /// Board can be invalid, e.g. has no king
        /// </summary>
        public bool Testing { get; set; } = false;
        
        public TranspositionTables Transpositions { get; }

        /// <summary>
        /// Query all raw moves when position known. No need for border check.
        /// </summary>
        public StaticMoves RawMoves { get; }

        public PieceCache PieceCache { get; }
        
        public SharedData(bool initialize = true)
        {
            Transpositions = new TranspositionTables();
            RawMoves = new StaticMoves();
            PieceCache = new PieceCache();

            if (initialize)
            {
                Transpositions.Initialize();
                RawMoves.Initialize();
                PieceCache.Initialize();
            }
        }
    }
}