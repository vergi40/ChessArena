using vergiBlue.Algorithms;
using vergiBlue.BoardModel.Subsystems;

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
        /// Disabled until
        /// * Working correctly
        /// Speed boost is considerable
        /// </summary>
        public bool UseCachedAttackSquares { get; set; } = false;
        
        public TranspositionTables Transpositions { get; }
        
        public SharedData()
        {
            Transpositions = new TranspositionTables();
            Transpositions.Initialize();
        }
    }
}