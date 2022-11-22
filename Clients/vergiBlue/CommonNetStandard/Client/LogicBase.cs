using System;
using System.Collections.Generic;
using CommonNetStandard.Common;
using System.Threading.Tasks;
using System.Threading;
using CommonNetStandard.Interface;

namespace CommonNetStandard.Client
{
    /// <summary>
    /// Api definition to use vergiBlue client in ChessArena grpc communication
    /// </summary>
    public interface IAiClient
    {
        /// <summary>
        /// Create move in current game board. Update board
        /// </summary>
        IPlayerMove CreateMove();

        /// <summary>
        /// Receive opponent move in current game board. Update board
        /// </summary>
        /// <param name="opponentMove"></param>
        void ReceiveMove(IMove opponentMove);
    }

    /// <summary>
    /// Api definition to use vergiBlue with UCI protocol
    /// </summary>
    public interface IUciClient
    {
        void NewGame();
        void SetBoard(string startPosOrFenBoard, List<string> moves);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="searchInfoUpdate"></param>
        /// <param name="ct"></param>
        /// <returns>Best move as compact string</returns>
        Task<string> CreateSearchTask(UciGoParameters parameters, Action<string> searchInfoUpdate,
            CancellationToken ct);
    }
}
