namespace vergiBlue.BoardModel.Subsystems.TranspositionTables;

/// <summary>
/// Alpha-beta tree node types.
/// https://www.chessprogramming.org/Node_Types#CUT
/// </summary>
public enum NodeType
{
    Exact,

    /// <summary>
    /// Eval is at most alpha.
    /// All-nodes. Cut-node occured with upper bound beta. Every move from all-node needs to be searched. Node score >= score (at least equal to score). E.g. evaluation 5, lowerbound can be [5, 6, 7, 8, 9].
    /// </summary>
    UpperBound,
        
    /// <summary>
    /// Eval is at least beta.
    /// Cut-nodes. Beta cutoff occured. A minimum of 1 node at a cut-node needs to be searched. Node score at most equal to eval score. E.g. evaluation 5, lowerbound can be [1, 2, 3, 4, 5]
    /// </summary>
    LowerBound
}