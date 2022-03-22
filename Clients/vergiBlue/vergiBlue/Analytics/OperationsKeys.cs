namespace vergiBlue.Analytics
{
    public static class OperationsKeys
    {
        public static string CacheCheckUtilized => "CacheCheckCount";
        public static string EvaluationDone => "EvaluationCount";
        public static string CheckEvaluationDone => "CheckEvaluationCount";
        public static string Alpha => "AlphaCutoffCount";
        public static string Beta => "BetaCutoffCount";
        public static string PriorityMoveFound => "PriorityMoveCount";
        public static string TranspositionUsed => "TranspositionsUsedCount";
        public static string TimeElapsedMs => "TimeElapsedMs";
    }
}