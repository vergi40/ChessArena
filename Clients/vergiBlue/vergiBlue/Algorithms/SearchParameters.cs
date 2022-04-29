using System;
using System.Threading;
using CommonNetStandard.Common;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms;

public class SearchParameters
{
    /// <summary>
    /// Parameters supplied by user "go" command
    /// </summary>
    public UciGoParameters UciParameters { get;}

    /// <summary>
    /// Piped to write info strings to standard output on each search depth
    /// </summary>
    public Action<string> WriteToOutputAction { get; }
    public TurnStartInfo TurnStartInfo { get; set; }

    /// <summary>
    /// Stop search ASAP when cancelled by user "stop" command
    /// </summary>
    public CancellationToken StopSearchToken { get; }

    public SearchParameters(UciGoParameters parameters, Action<string> searchInfoUpdate, CancellationToken stopSearchToken)
    {
        UciParameters = parameters;
        WriteToOutputAction = searchInfoUpdate;
        StopSearchToken = stopSearchToken;
    }
}