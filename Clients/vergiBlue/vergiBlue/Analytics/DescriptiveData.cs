using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Analytics
{
    public interface IMinimalTurnData
    {
        Guid GameId { get; }

        /// <summary>
        /// Start from 1
        /// </summary>
        public int TurnNumber { get; }
        string BoardFen { get; }
        bool IsWhiteTurn { get; }
    }

    public interface IDescriptiveTurnData : IMinimalTurnData
    {
        MoveEvaluationData EvalData { get; set; }
    }

    /// <summary>
    /// Collect statistical information from single turn.
    /// Descriptive analysis - what happened in the past
    /// </summary>
    internal class DescriptiveTurnData : IDescriptiveTurnData
    {
        public Guid GameId { get; set; }
        public int TurnNumber { get; set; }

        public string BoardFen { get; set; } = "";
        public bool IsWhiteTurn { get; set; }

        public MoveEvaluationData EvalData { get; set; } = new();

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"Turn {TurnNumber}: White move: {IsWhiteTurn}. Board FEN after move: ");
            return builder.ToString();
        }
    }

    public class MoveEvaluationData
    {
        public List<(double eval, ISingleMove move)> WeightedMoves { get; set; } = new();

        // Room to expand
    }
}
