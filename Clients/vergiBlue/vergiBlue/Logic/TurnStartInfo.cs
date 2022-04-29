using System.Collections.Generic;
using CommonNetStandard.Interface;
using vergiBlue.Analytics;

namespace vergiBlue.Logic;

/// <summary>
/// Pre-algorithm infos
/// </summary>
public record TurnStartInfo(bool isWhiteTurn, IReadOnlyList<IMove> gameHistory, LogicSettings settings,
    DiagnosticsData previousMoveData, int? overrideSearchDepth = null)
{
    public bool IsSearchDepthFixed => overrideSearchDepth != null;

    public int SearchDepthFixed
    {
        get
        {
            if (overrideSearchDepth != null)
            {
                return overrideSearchDepth.Value;
            }

            return -1;
        }
    }
}