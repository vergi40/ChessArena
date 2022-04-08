namespace vergiBlue.BoardModel.Subsystems.TranspositionTables;

/// <summary>
/// Store information of one board during certain depth
/// https://www.chessprogramming.org/Transposition_Table
/// </summary>
public class Transposition
{
    /// <summary>
    /// One-direction hash value for each possibly board situation. If two hashes are same, they have
    /// * Identical piece setup
    /// * Same player turn
    /// * Same castling rights
    /// * Same en passant situation
    /// </summary>
    public ulong Hash { get; set; }
    public int Depth { get; set; }
    public double Evaluation { get; set; }
        
    /// <summary>
    /// Is transposition evaluation from exact result, of some approximation.
    /// </summary>
    public NodeType Type { get; set; }
        
    /// <summary>
    /// Turn count in main board when transposition was saved.
    /// Used to delete old entries.
    /// </summary>
    public int GameTurnCount { get; set; }

    public Transposition(ulong hash, int depth, double evaluation, NodeType nodetype, int gameTurnCount)
    {
        Hash = hash;
        Depth = depth;
        Evaluation = evaluation;
        Type = nodetype;
        GameTurnCount = gameTurnCount;
    }

    public override string ToString()
    {
        return $"Eval: {Evaluation} - {Type.ToString()}";
    }
}