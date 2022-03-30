using System;
using System.Collections.Generic;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public interface IBoard
    {
        /// <summary>
        /// All pieces.
        /// Required features used in minimax:
        /// Remove piece with position or reference
        /// Add
        /// Get all black or white
        /// Sum all pieces
        /// https://stackoverflow.com/questions/454916/performance-of-arrays-vs-lists
        /// </summary>
        List<IPiece> PieceList { get; set; }

        /// <summary>
        /// Track kings at all times
        /// </summary>
        (IPiece? white, IPiece? black) Kings { get; set; }

        /// <summary>
        /// Single direction board information. Two hashes match if all pieces are in same position.
        /// </summary>
        ulong BoardHash { get; set; }

        /// <summary>
        /// Data reference where all transposition tables etc. should be fetched. Same data shared between all board instances.
        /// </summary>
        SharedData Shared { get; }

        /// <summary>
        /// Data reference where all measures, counters etc. should be stored. Strategic data is calculated in each initialization and move.
        /// Each new board has unique strategic data.
        /// </summary>
        StrategicData Strategic { get; }

        /// <summary>
        /// Return pieces in the <see cref="IPiece"/> format
        /// </summary>
        IReadOnlyList<IPiece> InterfacePieces { get; }

        MoveGenerator MoveGenerator { get; }

        // Functionality

        void InitializeDefaultBoard();

        /// <summary>
        /// Prerequisite: Pieces are set. Castling rights and en passant set.
        /// </summary>
        void InitializeSubSystems();

        /// <summary>
        /// Apply single move to board.
        /// Before executing, following should be applied:
        /// * <see cref="CollectMoveProperties(vergiBlue.SingleMove)"/>
        /// * (In desktop) UpdateGraphics()
        /// </summary>
        /// <param name="move"></param>
        /// <exception cref="InvalidMoveException"></exception>
        void ExecuteMoveWithValidation(in ISingleMove move);

        /// <summary>
        /// Apply single move to board. Most important function to keep consistent and error free.
        /// Assumes that the SingleMove-object has all properties up to date
        /// Actions in order:
        /// * Update board hash
        /// * Update capture
        ///   * Update en passant
        ///   * Remove opponent piece
        ///   * If king (shouldn't happen) -> game end -> DebugPostCheckMate = true
        /// * Update en passant status
        /// * Check castling status
        /// * Update castling status
        /// * Update piece position
        /// * Update endgame weight
        /// * Update turn count
        /// 
        /// Before executing, following should be applied:
        /// * <see cref="Board.CollectMoveProperties(vergiBlue.SingleMove)"/>
        /// * (In desktop) UpdateGraphics()
        /// </summary>
        /// <param name="move"></param>
        void ExecuteMove(in ISingleMove move);

        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        IPiece? KingLocation(bool whiteKing);

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        IPiece? ValueAt((int column, int row) target);

        /// <summary>
        /// Return piece at coordinates. Known to have value
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        IPiece ValueAtDefinitely((int column, int row) target);

        void AddNew(IPiece piece);
        void AddNew(IEnumerable<IPiece> pieces);
        void AddNew(params IPiece[] pieces);
        double Evaluate(bool isMaximizing, bool simpleEvaluation, int? currentSearchDepth = null);

        /// <summary>
        /// Checkmate or stalemate
        /// </summary>
        double EvaluateNoMoves(bool noMovesForWhite, bool simpleEvaluation, int? currentSearchDepth = null);


        /// <summary>
        /// If there is a player move that can eat other player king, and opponent has zero
        /// moves to stop this, it is checkmate
        /// </summary>
        /// <param name="isWhiteOffensive"></param>
        /// <param name="currentBoardKnownToBeInCheck">If already calculated, save some processing overhead</param>
        /// <returns></returns>
        bool IsCheckMate(bool isWhiteOffensive, bool currentBoardKnownToBeInCheck);

        /// <summary>
        /// If there is a player move that can eat other player king with current board setup, it is check.
        /// </summary>
        /// <param name="isWhiteOffensive">Which player moves are analyzed against others king checking</param>
        /// <returns></returns>
        bool IsCheck(bool isWhiteOffensive);

        /// <summary>
        /// Collect before the moves are executed to board.
        /// NOTE: time consuming, only use in upper level
        /// Prerequisite: move is valid
        /// </summary>
        IEnumerable<SingleMove> CollectMoveProperties(IEnumerable<SingleMove> moves);

        /// <summary>
        /// Collect before the move is executed to board.
        /// Any changes to board should be done in <see cref="Board.ExecuteMove"/>
        /// Prerequisite: move is valid
        /// </summary>
        SingleMove CollectMoveProperties(SingleMove initialMove);
        
        /// <summary>
        /// Return all valid moves the chosen color can do at current board
        /// </summary>
        IEnumerable<SingleMove> FilterOutIllegalMoves(IEnumerable<SingleMove> moves, bool isWhite);

        /// <summary>
        /// WARNING: Performance-heavy
        /// </summary>
        IEnumerable<(int column, int row)> GetAttackSquares(bool forWhiteAttacker);
    }
}