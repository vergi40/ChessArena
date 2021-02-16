﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard;
using CommonNetStandard.Interface;
using vergiBlue.Algorithms;
using vergiBlue.Pieces;

namespace vergiBlue
{
    public class DataInstance
    {
        public TranspositionTables Transpositions { get; }

        public DataInstance()
        {
            Transpositions = new TranspositionTables();
            Transpositions.Initialize();
        }
    }
    
    public class Board
    {
        /// <summary>
        /// [column,row}
        /// </summary>
        private PieceBase?[,] BoardArray { get; }

        /// <summary>
        /// All pieces.
        /// Required features used in minimax:
        /// Remove piece with position or reference
        /// Add
        /// Get all black or white
        /// Sum all pieces
        /// https://stackoverflow.com/questions/454916/performance-of-arrays-vs-lists
        /// </summary>
        public List<PieceBase> PieceList { get; set; }

        /// <summary>
        /// Track kings at all times
        /// </summary>
        public (PieceBase? white, PieceBase? black) Kings { get; set; }
        
        /// <summary>
        /// Single direction board information. Two hashes match if all pieces are in same position.
        /// </summary>
        public ulong BoardHash { get; set; }
        
        /// <summary>
        /// Data reference where all transposition tables etc. should be fetched. Same data shared between all board instances.
        /// </summary>
        public DataInstance SharedData { get; }

        /// <summary>
        /// Return pieces in the <see cref="IPiece"/> format
        /// </summary>
        public IList<IPiece> InterfacePieces
        {
            get
            {
                IList<IPiece> list = new List<IPiece>();
                foreach (var piece in PieceList)
                {
                    list.Add(piece);
                }

                return list;
            }
        }

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board()
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();

            SharedData = new DataInstance();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        /// <param name="previous"></param>
        public Board(Board previous)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            
            InitializeFromReference(previous);

            SharedData = previous.SharedData;
            // Create new hash as tests might not initialize board properly
            BoardHash = SharedData.Transpositions.CreateBoardHash(this);
        }

        /// <summary>
        /// Create board setup after move
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="move"></param>
        public Board(Board previous, SingleMove move)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            
            InitializeFromReference(previous);
            SharedData = previous.SharedData;
            BoardHash = previous.BoardHash;
            
            ExecuteMove(move);
        }
        
        /// <summary>
        /// Apply single move to board.
        /// </summary>
        /// <param name="move"></param>
        public void ExecuteMove(SingleMove move)
        {
            BoardHash = SharedData.Transpositions.GetNewBoardHash(move, this, BoardHash);
            var piece = ValueAt(move.PrevPos);
            if (piece == null) throw new ArgumentException($"Tried to execute move where previous piece position was empty ({move.PrevPos}).");

            if (move.Capture)
            {
                // Ensure validation ends if king is eaten
                var isWhite = piece.IsWhite;
                if (KingLocation(!isWhite)?.CurrentPosition == move.NewPos)
                {
                    RemovePieces(!isWhite);
                }
                else
                {
                    RemovePiece(move.NewPos);
                }
            }
            
            UpdatePosition(piece, move);
        }

        private void UpdatePosition(PieceBase piece, SingleMove move)
        {
            if (move.Promotion)
            {
                RemovePiece(piece.CurrentPosition);
                piece = new Queen(piece.IsWhite, move.NewPos);
                PieceList.Add(piece);
            }
            else
            {
                piece.CurrentPosition = move.NewPos;
            }

            BoardArray[move.PrevPos.Item1, move.PrevPos.Item2] = null;
            BoardArray[move.NewPos.Item1, move.NewPos.Item2] = piece;
        }

        private void RemovePiece((int column, int row) position)
        {
            var piece = ValueAt(position);
            if (piece == null) throw new ArgumentException($"Piece in position {position} was null");
            BoardArray[position.column, position.row] = null;

            PieceList.Remove(piece);
        }
        
        private void RemovePieces(bool isWhite)
        {
            var toBeRemoved = PieceList.Where(p => p.IsWhite == isWhite).ToList();
            foreach (var piece in toBeRemoved)
            {
                RemovePiece(piece.CurrentPosition);
            }
        }

        private void InitializeFromReference(Board previous)
        {
            foreach (var piece in previous.PieceList)
            {
                var newPiece = piece.CreateCopy();
                AddNew(newPiece);
                
                if (newPiece.Identity == 'K')
                {
                    UpdateKingReference(newPiece);
                }

            }
        }

        private void UpdateKingReference(PieceBase king)
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
        public PieceBase? ValueAt((int, int) target)
        {
            return BoardArray[target.Item1, target.Item2];
        }

        /// <summary>
        /// Return piece at coordinates. Known to have value
        /// </summary>
        /// <returns>Can be null</returns>
        public PieceBase ValueAtDefinitely((int, int) target)
        {
            var piece = BoardArray[target.Item1, target.Item2];
            if (piece == null) throw new ArgumentException($"Logical error. Value should not be null at {target.ToAlgebraic()}");

            return piece;
        }

        public void AddNew(PieceBase piece)
        {
            PieceList.Add(piece);
            BoardArray[piece.CurrentPosition.column, piece.CurrentPosition.row] = piece;

            if (piece.Identity == 'K')
            {
                UpdateKingReference(piece);
            }
        }

        public void AddNew(IEnumerable<PieceBase> pieces)
        {
            foreach (var piece in pieces)
            {
                AddNew(piece);
            }
        }

        public void AddNew(params PieceBase[] pieces)
        {
            foreach (var piece in pieces)
            {
                AddNew(piece);
            }
        }

        public double Evaluate(bool isMaximizing, bool simpleEvaluation, int? currentSearchDepth = null,
            int? moveCount = null)
        {
            if (simpleEvaluation) return EvaluateSimple(isMaximizing, currentSearchDepth, moveCount);
            return EvaluateIntelligent(isMaximizing, currentSearchDepth, moveCount);
        }


        public double EvaluateSimple(bool isMaximizing, int? currentSearchDepth = null, int? moveCount = null)
        {
            Diagnostics.IncrementEvalCount();
            var evalScore = PieceList.Sum(p => p.RelativeStrength);

            // Checkmate (in good or bad) should have more priority the sooner it occurs
            if(currentSearchDepth != null)
            {
                if (evalScore > PieceBaseStrength.King / 2)
                {
                    evalScore += 10 * (currentSearchDepth.Value + 1);
                }
                else if (evalScore < -PieceBaseStrength.King / 2)
                {
                    evalScore -= 10 * (currentSearchDepth.Value + 1);
                }
            }

            return evalScore;
        }

        public double EvaluateIntelligent(bool isMaximizing, int? currentSearchDepth = null, int? moveCount = null)
        {
            // TODO position evaluation
            // TODO pawn structure
            // TODO king position

            Diagnostics.IncrementEvalCount();
            var evalScore = PieceList.Sum(p => p.PositionStrength);

            // Checkmate (in good or bad) should have more priority the sooner it occurs
            if (currentSearchDepth != null)
            {
                if (evalScore > PieceBaseStrength.King / 2)
                {
                    evalScore += 10 * (currentSearchDepth.Value + 1);
                }
                else if (evalScore < -PieceBaseStrength.King / 2)
                {
                    evalScore -= 10 * (currentSearchDepth.Value + 1);
                }
            }

            return evalScore;
        }

        /// <summary>
        /// Adds capture-moves in front
        /// </summary>
        public IList<SingleMove> Moves(bool forWhite, bool orderMoves, bool kingInDanger = false)
        {
            IList<SingleMove> list = new List<SingleMove>();
            foreach (var piece in PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(this))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = new Board(this, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    list.Add(singleMove);
                }
            }

            if (orderMoves) return MoveOrdering.SortMovesByEvaluation(list, this, forWhite);
            else return MoveOrdering.SortMovesByGuessWeight(list, this, forWhite);
        }

        public IList<SingleMove> MovesWithTranspositionOrder(bool forWhite, bool kingInDanger = false)
        {
            // Priority moves like known cutoffs
            var priorityList = new List<SingleMove>();
            var otherList = new List<SingleMove>();
            foreach (var piece in PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(this))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = new Board(this, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    // Check if move has transposition data
                    // Maximizing player needs lower bound moves
                    // Minimizing player needs upper bound moves
                    var transposition = SharedData.Transpositions.GetTranspositionForMove(this, singleMove);
                    if (transposition != null)
                    {
                        if((forWhite && transposition.Type == NodeType.LowerBound) ||
                            (!forWhite && transposition.Type == NodeType.UpperBound))
                        {
                            Diagnostics.IncrementPriorityMoves();
                            priorityList.Add(singleMove);
                            continue;
                        }
                    }
                    otherList.Add(singleMove);
                }
            }

            priorityList.AddRange(MoveOrdering.SortMovesByGuessWeight(otherList, this, forWhite));
            return priorityList;
        }

        public void InitializeEmptyBoard()
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
            Kings = (whiteKing, blackKing);

            SharedData.Transpositions.Initialize();
            BoardHash = SharedData.Transpositions.CreateBoardHash(this);
            Logger.Log("Board initialized.");
        }

        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        private PieceBase? KingLocation(bool whiteKing)
        {
            if (whiteKing) return Kings.white;
            else return Kings.black;
        }

        /// <summary>
        /// If any player move could eat other player king, and opponent has zero
        /// moves to stop this, it is checkmate
        /// </summary>
        /// <param name="isWhiteOffensive"></param>
        /// <param name="currentBoardKnownToBeInCheck">If already calculated, save some processing overhead</param>
        /// <returns></returns>
        public bool IsCheckMate(bool isWhiteOffensive, bool currentBoardKnownToBeInCheck)
        {
            if(!currentBoardKnownToBeInCheck)
            {
                if (!IsCheck(isWhiteOffensive)) return false;
            }

            // Iterate all opponent moves and check is there any that doesn't have check when next player moves
            var opponentMoves = Moves(!isWhiteOffensive, false);
            foreach (var singleMove in opponentMoves)
            {
                var newBoard = new Board(this, singleMove);
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
        /// If any player move could eat other player king with current board setup, it is check.
        /// </summary>
        /// <param name="isWhiteOffensive">Which player moves are analyzed against others king checking</param>
        /// <returns></returns>
        public bool IsCheck(bool isWhiteOffensive)
        {
            var opponentKing = KingLocation(!isWhiteOffensive);
            if (opponentKing == null) return false; // Test override, don't always have kings on board

            var playerMoves = Moves(isWhiteOffensive, false, false);
            foreach (var singleMove in playerMoves)
            {
                Diagnostics.IncrementCheckCount();
                if (singleMove.NewPos == opponentKing.CurrentPosition)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
