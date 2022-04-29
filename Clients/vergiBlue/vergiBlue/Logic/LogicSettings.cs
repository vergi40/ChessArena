namespace vergiBlue.Logic
{
    public class LogicSettings
    {
        // Config bools. Default values used in real game
        public bool UseTranspositionTables { get; set; } = true;

        public int TimeLimitInMs { get; set; } = 4000;

        public bool UseParallelComputation { get; set; } = false;
        public bool UseIterativeDeepening { get; set; } = true;

        /// <summary>
        /// Log more data, like alpha-betas
        /// </summary>
        public bool UseFullDiagnostics { get; set; } = true;

        public int ClearSavedTranspositionsAfterTurnsPassed { get; set; } = 4;

    }
}