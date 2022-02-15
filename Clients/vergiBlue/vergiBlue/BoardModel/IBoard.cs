﻿using System;
using System.Collections.Generic;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
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
        List<PieceBase> PieceList { get; set; }

        /// <summary>
        /// Track kings at all times
        /// </summary>
        (PieceBase? white, PieceBase? black) Kings { get; set; }

        /// <summary>
        /// Single direction board information. Two hashes match if all pieces are in same position.
        /// </summary>
        ulong BoardHash { get; set; }

        /// <summary>
        /// Board that was in checkmate was continued
        /// </summary>
        bool DebugPostCheckMate { get; }

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
        IList<IPiece> InterfacePieces { get; }

        /// <summary>
        /// Apply single move to board.
        /// Before executing, following should be applied:
        /// * <see cref="CollectMoveProperties(vergiBlue.SingleMove)"/>
        /// * (In desktop) UpdateGraphics()
        /// </summary>
        /// <param name="move"></param>
        /// <exception cref="InvalidMoveException"></exception>
        void ExecuteMoveWithValidation(SingleMove move);

        /// <summary>
        /// Apply single move to board.
        /// Before executing, following should be applied:
        /// * <see cref="Board.CollectMoveProperties(vergiBlue.SingleMove)"/>
        /// * (In desktop) UpdateGraphics()
        /// </summary>
        /// <param name="move"></param>
        void ExecuteMove(SingleMove move);

        void UpdateEndGameWeight();

        /// <summary>
        /// How many percent of non-pawn pieces exists on board
        /// </summary>
        /// <returns></returns>
        double GetPowerPiecePercent();

        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        PieceBase? KingLocation(bool whiteKing);

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        PieceBase? ValueAt((int column, int row) target);

        /// <summary>
        /// Return piece at coordinates. Known to have value
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        PieceBase ValueAtDefinitely((int column, int row) target);

        void AddNew(PieceBase piece);
        void AddNew(IEnumerable<PieceBase> pieces);
        void AddNew(params PieceBase[] pieces);
        double Evaluate(bool isMaximizing, bool simpleEvaluation, bool isInCheckForOther = false, int? currentSearchDepth = null);

        /// <summary>
        /// Find every possible move for every piece for given color.
        /// </summary>
        IList<SingleMove> Moves(bool forWhite, bool orderMoves, bool kingInDanger = false);

        IList<SingleMove> MovesWithTranspositionOrder(bool forWhite, bool kingInDanger = false);
        void InitializeDefaultBoard();

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
        /// Prerequisite: move is valid
        /// </summary>
        SingleMove CollectMoveProperties(SingleMove move);

        /// <summary>
        /// Collect before the move is executed to board.
        /// Any changes to board should be done in <see cref="Board.ExecuteMove"/>
        /// </summary>
        /// <returns></returns>
        SingleMove CollectMoveProperties((int column, int row) from, (int column, int row) to);

        /// <summary>
        /// Return all valid moves the chosen color can do at current board
        /// </summary>
        IEnumerable<SingleMove> FilterOutIllegalMoves(IEnumerable<SingleMove> moves, bool isWhite);
        IList<(int column, int row)> GetAttackSquares(bool attackerColor);
        bool CanCastleToLeft(bool white);
        bool CanCastleToRight(bool white);
    }
}