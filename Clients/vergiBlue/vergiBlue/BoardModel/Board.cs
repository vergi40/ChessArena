﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Interface;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Analytics;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public class Board : IBoard
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Board>();

        /// <summary>
        /// [column,row}
        /// </summary>
        private IPiece?[] BoardArray { get; } = new IPiece?[64];

        /// <summary>
        /// All pieces.
        /// Required features used in minimax:
        /// Remove piece with position or reference
        /// Add
        /// Get all black or white
        /// Sum all pieces
        /// https://stackoverflow.com/questions/454916/performance-of-arrays-vs-lists
        /// </summary>
        public List<IPiece> PieceList { get; }

        /// <summary>
        /// Track kings at all times
        /// </summary>
        public (IPiece? white, IPiece? black) Kings { get; set; }
        
        /// <summary>
        /// Single direction board information. Two hashes match if all pieces are in same position.
        /// </summary>
        public ulong BoardHash { get; set; }
        
        /// <summary>
        /// Data reference where all transposition tables etc. should be fetched. Same data shared between all board instances.
        /// </summary>
        public SharedData Shared { get; }

        /// <summary>
        /// Data reference where all measures, counters etc. should be stored. Strategic data is calculated in each initialization and move.
        /// Each new board has unique strategic data.
        /// </summary>
        public StrategicData Strategic { get; }

        /// <summary>
        /// Return pieces in the <see cref="IPiece"/> format
        /// </summary>
        public IReadOnlyList<IPieceMinimal> InterfacePieces
        {
            get
            {
                var list = new List<IPiece>();
                foreach (var piece in PieceList)
                {
                    list.Add(piece);
                }

                return list;
            }
        }

        /// <summary>
        /// Is black/white check precalculated and what is the result
        /// [0] black, [1] white
        /// bool?: null - not calculated / false - no / true - yes
        /// </summary>
        private bool?[] _isCheck { get; } = new bool?[2];
        
        public MoveGenerator MoveGenerator { get; }

        public PieceQuery PieceQuery { get; }

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board(bool initializeShared = true)
        {
            PieceList = new List<IPiece>();
            MoveGenerator = new MoveGenerator(this);
            PieceQuery = new PieceQuery(this);
            Shared = new SharedData(initializeShared);
            Strategic = new StrategicData();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public Board(IBoard other, bool cloneSubSystems)
        {
            // Minor optimization. PieceList should never grow
            PieceList = new List<IPiece>(other.PieceList.Count);

            MoveGenerator = new MoveGenerator(this);
            PieceQuery = new PieceQuery(this);
            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);

            InitializeFromReference(other);

            if (cloneSubSystems)
            {
                BoardHash = other.BoardHash;
            }
            else
            {
                InitializeSubSystems();
            }

            UpdateEndGameWeight();
        }
        
        public Board(IBoard other, in ISingleMove move)
        {
            PieceList = new List<IPiece>();
            MoveGenerator = new MoveGenerator(this);
            PieceQuery = new PieceQuery(this);
            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);

            InitializeFromReference(other);
            BoardHash = other.BoardHash;

            ExecuteMove(move);
        }

        /// <summary>
        /// Prerequisite: Pieces are set. Castling rights and en passant set.
        /// </summary>
        public void InitializeSubSystems()
        {
            Shared.Transpositions.Initialize();
            BoardHash = Shared.Transpositions.CreateBoardHash(this);
        }

        private void InitializeFromReference(IBoard previous)
        {
            foreach (var piece in previous.PieceList)
            {
                AddNew(piece);
            }
        }

        public void InitializeDefaultBoard()
        {
            // Pawns
            for (int i = 0; i < 8; i++)
            {
                var whitePawn = new Pawn(true, (i, 1));
                AddNew(whitePawn);

                var blackPawn = new Pawn(false, (i, 6));
                AddNew(blackPawn);
            }

            var rooks = new List<Rook>
            {
                new Rook(true, "a1"),
                new Rook(true, "h1"),
                new Rook(false, "a8"),
                new Rook(false, "h8")
            };
            AddNew(rooks);

            var knights = new List<Knight>
            {
                new Knight(true, "b1"),
                new Knight(true, "g1"),
                new Knight(false, "b8"),
                new Knight(false, "g8")
            };
            AddNew(knights);

            var bishops = new List<Bishop>
            {
                new Bishop(true, "c1"),
                new Bishop(true, "f1"),
                new Bishop(false, "c8"),
                new Bishop(false, "f8")
            };
            AddNew(bishops);

            var queens = new List<Queen>
            {
                new Queen(true, "d1"),
                new Queen(false, "d8")
            };
            AddNew(queens);

            var whiteKing = new King(true, "e1");
            AddNew(whiteKing);

            var blackKing = new King(false, "e8");
            AddNew(blackKing);

            InitializeSubSystems();
        }

        public void ExecuteMoveWithValidation(in ISingleMove move)
        {
            Validator.ValidateMove(this, move);
            ExecuteMove(move);
        }
        
        public void ExecuteMove(in ISingleMove move)
        {
            BoardHash = Shared.Transpositions.GetNewBoardHash(move, this, BoardHash);

            var piece = ValueAt(move.PrevPos);
            if (piece == null) throw new ArgumentException($"Tried to execute move where previous piece position was empty ({move.PrevPos}).");

            if (move.Capture)
            {
                var isWhite = piece.IsWhite;
                if (move.EnPassant)
                {
                    RemovePiece(move.EnPassantOpponentPosition);
                }
                else if (KingLocation(!isWhite)?.CurrentPosition == move.NewPos)
                {
                    throw new ArgumentException("Logical error: king was captured");
                }
                else
                {
                    RemovePiece(move.NewPos);
                }
            }

            Strategic.UpdateEnPassantStatus(move, piece);

            if (move.Castling)
            {
                Strategic.UpdateCastlingStatusFromMove(move);
            }
            else
            {
                Castling.UpdateStatusForNonCastling(this, piece, move);
            }

            UpdatePieceFromMoveInternal(piece, move);

            // Initialize cache values that depend on board setup
            MoveGenerator.SliderAttacksCached = null;
            _isCheck[0] = null;
            _isCheck[1] = null;

            // General every turn processes
            UpdateEndGameWeight();
            Strategic.TurnCountInCurrentDepth++;
        }

        /// <summary>
        /// NOTE: definition - to some general class
        /// </summary>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        protected static int ColorToInt(bool isWhite)
        {
            return isWhite ? 1 : 0;
        }

        public void UpdateEndGameWeight()
        {
            // TODO pawns

            Strategic.EndGameWeight = 1 - GetPowerPiecePercent();
        }
        
        /// <summary>
        /// How many percent of non-pawn pieces exists on board
        /// </summary>
        /// <returns></returns>
        public double GetPowerPiecePercent()
        {
            var powerPieces = PieceQuery.AllPowerPiecesList().Count;
            return powerPieces * 0.0625;
        }
        
        /// <summary>
        /// Only piece itself & castling. No capture logic.
        /// Remove old position from array.
        /// Remove old from PieceList.
        /// Add new position to array.
        /// Add new position to PieceList.
        /// </summary>
        /// <param name="oldPiece"></param>
        /// <param name="move"></param>
        /// <exception cref="ArgumentException"></exception>
        private void UpdatePieceFromMoveInternal(IPiece oldPiece, in ISingleMove move)
        {
            RemovePiece(move.PrevPos);

            IPiece newPiece;
            if (move.PromotionType != PromotionPieceType.NoPromotion)
            {
                newPiece = move.PromotionType switch
                {
                    PromotionPieceType.Queen => Shared.PieceCache.Get(move.NewPos, 'Q', oldPiece.IsWhite),
                    PromotionPieceType.Rook => Shared.PieceCache.Get(move.NewPos, 'R', oldPiece.IsWhite),
                    PromotionPieceType.Bishop => Shared.PieceCache.Get(move.NewPos, 'B', oldPiece.IsWhite),
                    PromotionPieceType.Knight => Shared.PieceCache.Get(move.NewPos, 'N', oldPiece.IsWhite),
                    _ => throw new ArgumentException($"Unknown promotion: {move.PromotionType}")
                };
            }
            else
            {
                newPiece = Shared.PieceCache.Get(move.NewPos, oldPiece.Identity, oldPiece.IsWhite);
            }

            if (move.Castling)
            {
                // Most definitely known that king and rook are untouched and in place
                if (move.NewPos.column == 2)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    var rookMove = new SingleMove((0, row), (3, row));
                    UpdatePieceFromMoveInternal(ValueAtDefinitely((0, row)), rookMove);
                }
                if (move.NewPos.column == 6)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    var rookMove = new SingleMove((7, row), (5, row));
                    UpdatePieceFromMoveInternal(ValueAtDefinitely((7, row)), rookMove);
                }
            }

            AddNew(newPiece);
        }

        /// <summary>
        /// Cast IBoard to Board to use this in testing.
        /// If there is any piece in target square, it's deleted
        /// </summary>
        /// <param name="move"></param>
        public void UpdateBoardArray(in ISingleMove move)
        {
            var piece = ValueAtDefinitely(move.PrevPos);

            var toBeDeleted = ValueAt(move.NewPos);
            if (toBeDeleted != null)
            {
                RemovePiece(move.NewPos);
            }

            UpdatePieceFromMoveInternal(piece, move);
        }

        /// <summary>
        /// Remove from board array. Remove from PieceList
        /// </summary>
        public void RemovePiece((int column, int row) position)
        {
            var piece = ValueAt(position);
            if (piece == null) throw new ArgumentException($"Piece in position {position} was null");
            BoardArray[position.To1DimensionArray()] = null;

            PieceList.Remove(piece);
            if (piece.Identity == 'K')
            {
                if (piece.IsWhite)
                {
                    Kings = (null, Kings.black);
                }
                else
                {
                    Kings = (Kings.white, null);
                }
            }
        }
        
        private void UpdateKingReference(IPiece king)
        {
            if (king.IsWhite)
            {
                Kings = (king, Kings.black);
            }
            else
            {
                Kings = (Kings.white, king);
            }
        }
        
        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        public IPiece? ValueAt((int column, int row) target)
        {
            return BoardArray[target.To1DimensionArray()];
        }

        /// <summary>
        /// Return piece at coordinates. Known to have value
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public IPiece ValueAtDefinitely((int column, int row) target)
        {
            var piece = BoardArray[target.To1DimensionArray()];
            if (piece == null) throw new ArgumentException($"Logical error. Value should not be null at {target.ToAlgebraic()}");

            return piece;
        }

        /// <summary>
        /// Add piece to array and PieceList. Update king references
        /// </summary>
        /// <param name="piece"></param>
        public void AddNew(IPiece piece)
        {
            PieceList.Add(piece);
            BoardArray[piece.CurrentPosition.To1DimensionArray()] = piece;

            if (piece.Identity == 'K')
            {
                UpdateKingReference(piece);
            }
        }

        public void AddNew(IEnumerable<IPiece> pieces)
        {
            foreach (var piece in pieces)
            {
                AddNew(piece);
            }
        }

        public void AddNew(params IPiece[] pieces)
        {
            foreach (var piece in pieces)
            {
                AddNew(piece);
            }
        }

        public double Evaluate(bool isMaximizing, bool simpleEvaluation,
            int? currentSearchDepth = null)
        {
            return Evaluator.Evaluate(this, isMaximizing, simpleEvaluation, currentSearchDepth);
        }

        public double EvaluateNoMoves(bool noMovesForWhite, bool simpleEvaluation, int? currentSearchDepth = null)
        {
            return Evaluator.EvaluateNoMoves(this, noMovesForWhite, simpleEvaluation, currentSearchDepth);
        }
        
        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        public IPiece? KingLocation(bool whiteKing)
        {
            if (whiteKing) return Kings.white;
            else return Kings.black;
        }

        /// <summary>
        /// If there is a player move that can eat other player king, and opponent has zero
        /// moves to stop this, it is checkmate
        /// </summary>
        /// <param name="isWhiteOffensive"></param>
        /// <param name="currentBoardKnownToBeInCheck">If already calculated, save some processing overhead</param>
        /// <returns></returns>
        public bool IsCheckMate(bool isWhiteOffensive, bool currentBoardKnownToBeInCheck)
        {
            if(!currentBoardKnownToBeInCheck)
            {
                // Not even check currently - cancel
                if (!IsCheck(isWhiteOffensive)) return false;
            }

            // Iterate all opponent moves and check is there any that doesn't have check when next player moves
            // No need to include castling as checked king cannot castle
            var opponentMoves = MoveGenerator.ValidMovesQuickWithoutCastling(!isWhiteOffensive);
            foreach (var singleMove in opponentMoves)
            {
                var newBoard = BoardFactory.CreateFromMove(this, singleMove);
                if (!newBoard.IsCheck(isWhiteOffensive))
                {
                    // Found possible move
                    // TODO should this be saved for some deep analyzing?
                    return false;
                }
            }

            // Didn't find any counter moves
            return true;
        }

        /// <summary>
        /// If there is a player move that can eat other player king with current board setup, it is check.
        /// </summary>
        /// <param name="isWhiteOffensive">Which player moves are analyzed against others king checking</param>
        /// <returns></returns>
        public bool IsCheck(bool isWhiteOffensive)
        {
            var preCalculated = _isCheck[ColorToInt(!isWhiteOffensive)];
            if (preCalculated != null)
            {
                Collector.IncreaseOperationCount(OperationsKeys.CacheCheckUtilized);
                return preCalculated.Value;
            }

            Collector.IncreaseOperationCount(OperationsKeys.CheckEvaluationDone);
            if(MoveGenerator.IsKingCurrentlyAttacked(!isWhiteOffensive))
            {
                _isCheck[ColorToInt(!isWhiteOffensive)] = true;
                return true;
            }

            _isCheck[ColorToInt(!isWhiteOffensive)] = false;
            return false;
        }

        public IEnumerable<SingleMove> CollectMoveProperties(IEnumerable<SingleMove> moves)
        {
            foreach (var basicMove in moves)
            {
                yield return CollectMoveProperties(basicMove);
            }
        }

        public SingleMove CollectMoveProperties(SingleMove initialMove)
        {
            (int column, int row) from = initialMove.PrevPos;
            (int column, int row) to = initialMove.NewPos;

            var ownPiece = ValueAtDefinitely(from);
            var isWhite = ownPiece.IsWhite;

            var move = new SingleMove(from, to);
            
            if (initialMove.Capture)
            {
                // Known capture or en passant
                move.Capture = true;
                if (initialMove.EnPassant)
                {
                    move.EnPassant = true;
                }
            }
            else
            {
                // Unknown, check possibilities
                var opponentPiece = ValueAt(to);
                if (opponentPiece != null)
                {
                    if (opponentPiece.IsWhite != isWhite)
                    {
                        move.Capture = true;
                    }
                    else throw new ArgumentException($"Player with white={isWhite} tried to capture own piece. {ownPiece.IsWhite}: {initialMove.ToString()}");
                }
                else if(ownPiece.Identity == 'P' && Strategic.EnPassantPossibility != null && to == Strategic.EnPassantPossibility.Value)
                {
                    move.Capture = true;
                    move.EnPassant = true;
                }
            }

            // Promotion
            move.PromotionType = initialMove.PromotionType;
            if (ownPiece.Identity == 'P' && move.PromotionType == PromotionPieceType.NoPromotion &&
                (to.row == 0 || to.row == 7))
            {
                // Missing promotion. Fix to queen
                move.PromotionType = PromotionPieceType.Queen;
            }

            // Castling
            if (initialMove.Castling)
            {
                move.Castling = true;
            }
            else
            {
                var castlingRow = 0;
                if (!isWhite) castlingRow = 7;
                if (ownPiece.Identity == 'K')
                {
                    if (move.PrevPos == (4, castlingRow) &&
                        (move.NewPos == (2, castlingRow) || move.NewPos == (6, castlingRow)))
                    {
                        move.Castling = true;
                    }
                }
            }

            // Check and checkmate
            var nextBoard = BoardFactory.CreateFromMove(this, move);
            move.Check = nextBoard.IsCheck(isWhite);
            move.CheckMate = nextBoard.IsCheckMate(isWhite, move.Check);
            return move;
        }

        public IEnumerable<SingleMove> FilterOutIllegalMoves(IEnumerable<SingleMove> moves, bool isWhite)
        {
            var legalMoves = MoveGenerator.ValidMovesQuick(isWhite).ToList();
            foreach (var singleMove in moves)
            {
                var isLegal =
                    legalMoves.FirstOrDefault(m => m.EqualPositions(singleMove));
                if(isLegal != null)
                {
                    // Move is ok
                    yield return singleMove;
                }
            }
        }
        
        public IEnumerable<(int column, int row)> GetAttackSquares(bool forWhiteAttacker)
        {
            // TODO separate method for capture moves and all moves
            foreach (var move in MoveGenerator.AttackMoves(forWhiteAttacker))
            {
                yield return move.NewPos;
            }
        }

        public string GenerateFen()
        {
            return "TODO";
        }
    }
}
