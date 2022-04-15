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


        /// <summary>
        /// For tests. Keep parameters intact. After logic constructor, this initialization can be used to set any logical aspect.
        /// LTS - Long Time Support. Parameters will be kept the same.
        /// </summary>
        /// <param name="useParallelComputation"></param>
        /// <param name="useTranspositionTables"></param>
        /// <param name="useIterativeDeepening"></param>
        public void SetConfigLTS(bool? useParallelComputation = null, bool? useTranspositionTables = null, bool? useIterativeDeepening = null, bool? useFullDiagnostics = null)
        {
            if (useParallelComputation != null) UseParallelComputation = useParallelComputation.Value;
            if (useTranspositionTables != null) UseTranspositionTables = useTranspositionTables.Value;
            if (useIterativeDeepening != null) UseIterativeDeepening = useIterativeDeepening.Value;
            if (useFullDiagnostics != null) UseFullDiagnostics = useFullDiagnostics.Value;
        }
    }
}