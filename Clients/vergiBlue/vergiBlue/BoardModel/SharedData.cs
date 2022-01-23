using vergiBlue.Algorithms;

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
        
        public TranspositionTables Transpositions { get; }
        
        public SharedData()
        {
            Transpositions = new TranspositionTables();
            Transpositions.Initialize();
        }
    }
}