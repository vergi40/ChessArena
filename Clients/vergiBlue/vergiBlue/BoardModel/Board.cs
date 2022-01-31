using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard;
using CommonNetStandard.Interface;
using log4net;
using vergiBlue.Algorithms;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public class Board : IBoard
    {
        private static readonly ILog _localLogger = LogManager.GetLogger(typeof(Board));
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
        public SharedData Shared { get; }

        /// <summary>
        /// Data reference where all measures, counters etc. should be stored. Strategic data is calculated in each initialization and move.
        /// Each new board has unique strategic data.
        /// </summary>
        public StrategicData Strategic { get; }

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
        /// Is check = true. Not calculated = null.
        /// Save some time if already calculated checkmate
        /// </summary>
        private bool? _isCheckForOffensivePrecalculated { get; set; } = null;

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board()
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();

            Shared = new SharedData();
            Strategic = new StrategicData();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public Board(IBoard other)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            
            InitializeFromReference(other);

            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);
            
            // Create new hash as tests might not initialize board properly
            BoardHash = Shared.Transpositions.CreateBoardHash(this);
            UpdateEndGameWeight();
        }

        /// <summary>
        /// Create board setup after move
        /// </summary>
        public Board(IBoard other, SingleMove move)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            
            InitializeFromReference(other);
            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);
            BoardHash = other.BoardHash;

            ExecuteMove(move);
        }

        public void ExecuteMoveWithValidation(SingleMove move)
        {
            Validator.ValidateMove(this, move);
            ExecuteMove(move);
        }
        
        public void ExecuteMove(SingleMove move)
        {
            BoardHash = Shared.Transpositions.GetNewBoardHash(move, this, BoardHash);
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

            if (move.Castling)
            {
                Strategic.UpdateCastlingStatusFromMove(move);
            }

            UpdateCastlingStatus(piece.IsWhite);
            
            UpdatePosition(piece, move);
            
            // General every turn processes
            UpdateEndGameWeight();
            Strategic.TurnCountInCurrentDepth++;
        }

        /// <summary>
        /// Check if castling pieces are still in place
        /// </summary>
        private void UpdateCastlingStatus(bool isWhite)
        {
            // TODO lot of statements, could use optimization
            var row = 0;
            if (isWhite && (Strategic.WhiteLeftCastlingValid || Strategic.WhiteRightCastlingValid))
            {
                // Castling pieces are intact
                var leftRook = ValueAt((0, row));
                var rightRook = ValueAt((7, row));
                var king = ValueAt((4, row));

                if (king == null || king.Identity != 'K')
                {
                    Strategic.WhiteLeftCastlingValid = false;
                    Strategic.WhiteRightCastlingValid = false;
                }

                if (leftRook == null || leftRook.Identity != 'R')
                {
                    Strategic.WhiteLeftCastlingValid = false;
                }
                if (rightRook == null || rightRook.Identity != 'R')
                {
                    Strategic.WhiteRightCastlingValid = false;
                }
            }
            else if(Strategic.BlackLeftCastlingValid || Strategic.BlackRightCastlingValid)
            {
                row = 7;
                // Castling pieces are intact
                var leftRook = ValueAt((0, row));
                var rightRook = ValueAt((7, row));
                var king = ValueAt((4, row));

                if (king == null || king.Identity != 'K')
                {
                    Strategic.BlackLeftCastlingValid = false;
                    Strategic.BlackRightCastlingValid = false;
                }

                if (leftRook == null || leftRook.Identity != 'R')
                {
                    Strategic.BlackLeftCastlingValid = false;
                }
                if (rightRook == null || rightRook.Identity != 'R')
                {
                    Strategic.BlackRightCastlingValid = false;
                }
            }
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
            var powerPieces = PieceList.Count(p => Math.Abs((double)p.RelativeStrength) > PieceBaseStrength.Pawn);
            return powerPieces / 16.0;
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

            if (move.Castling)
            {
                // Most definitely known that king and rook are untouched and in place
                if (move.NewPos.column == 2)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    var rookMove = new SingleMove((0, row), (3, row));
                    UpdatePosition(ValueAtDefinitely((0, row)), rookMove);
                }
                if (move.NewPos.column == 6)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    var rookMove = new SingleMove((7, row), (5, row));
                    UpdatePosition(ValueAtDefinitely((7, row)), rookMove);
                }
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

        private void InitializeFromReference(IBoard previous)
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
        public PieceBase? ValueAt((int column, int row) target)
        {
            return BoardArray[target.column, target.row];
        }

        /// <summary>
        /// Return piece at coordinates. Known to have value
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public PieceBase ValueAtDefinitely((int column, int row) target)
        {
            var piece = BoardArray[target.column, target.row];
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

        public double Evaluate(bool isMaximizing, bool simpleEvaluation, bool isInCheckForOther = false, int? currentSearchDepth = null)
        {
            if (simpleEvaluation) return EvaluateSimple(isMaximizing, currentSearchDepth);
            return EvaluateIntelligent(isMaximizing, isInCheckForOther, currentSearchDepth);
        }


        public double EvaluateSimple(bool isMaximizing, int? currentSearchDepth = null)
        {
            Diagnostics.IncrementEvalCount();
            var evalScore = PieceList.Sum(p => p.RelativeStrength);
            
            return evalScore;
        }

        public double EvaluateIntelligent(bool isMaximizing, bool isInCheckForOther, int? currentSearchDepth = null)
        {
            Diagnostics.IncrementEvalCount();
            var evalScore = PieceList.Sum(p => p.GetEvaluationStrength(Strategic.EndGameWeight));

            // Checkmate override
            // Equalize checkmate scores, so relative positions of other pieces don't effect outcome
            // Also give more priority for shallower moves.
            if (Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold)
            {
                if (currentSearchDepth != null)
                {
                    return MoveResearch.CheckMateScoreAdjustToDepthFixed(evalScore, currentSearchDepth.Value);
                }
                else
                {
                    return MoveResearch.CheckMateScoreAdjustToEven(evalScore);
                }
            }

            // Stalemate
            // TODO not working in endgame properly somehow
            //if (Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold && !isInCheckForOther)
            //{
            //    return 0;
            //    // Otherwise would be evaluated like -200000
            //}
            

            // TODO pawn structure
            // Separate start game weight functions

            if (Strategic.EndGameWeight > 0.50)
            {
                evalScore += EndGameKingToCornerEvaluation(isMaximizing);
            }

            

            return evalScore;
        }
        
        public double EndGameKingToCornerEvaluation(bool isWhite)
        {
            // TODO debug hack
            var ownPieces = PieceList.Where(p => p.IsWhite == isWhite).ToList();
            if (ownPieces.Count == 1)
            {
                return ownPieces.First().GetEvaluationStrength(-1);
            }

            var evaluation = 0.0;
            var opponentKing = KingLocation(!isWhite);
            var ownKing = KingLocation(isWhite);
            
            // Testing running
            if (opponentKing == null || ownKing == null) return 0.0;

            // In endgame, favor opponent king to be on edge of board
            double center = 3.5;
            var distanceToCenterRow = Math.Abs(center - opponentKing.CurrentPosition.row);
            var distanceToCenterColumn = Math.Abs(center - opponentKing.CurrentPosition.column);
            evaluation += 1 * (distanceToCenterRow + distanceToCenterColumn);
            
            // In endgame, favor own king closed to opponent to cut off escape routes
            var rowDifference = Math.Abs(ownKing.CurrentPosition.row - opponentKing.CurrentPosition.row);
            var columnDifference = Math.Abs(ownKing.CurrentPosition.column - opponentKing.CurrentPosition.column);
            var kingDifference = rowDifference + columnDifference;
            evaluation += 14 - kingDifference;

            evaluation += evaluation * 35 * Strategic.EndGameWeight;

            if (isWhite) return evaluation;
            else return -evaluation;
        }

        /// <summary>
        /// Find every possible move for every piece for given color.
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
                        var newBoard = BoardFactory.CreateFromMove(this, singleMove);
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
                        var newBoard = BoardFactory.CreateFromMove(this, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    // Check if move has transposition data
                    // Maximizing player needs lower bound moves
                    // Minimizing player needs upper bound moves
                    var transposition = Shared.Transpositions.GetTranspositionForMove(this, singleMove);
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
            Kings = (whiteKing, blackKing);

            Shared.Transpositions.Initialize();
            BoardHash = Shared.Transpositions.CreateBoardHash(this);
            Logger.LogWithConsole("Board initialized.", _localLogger);
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
                if (!IsCheck(isWhiteOffensive)) return false;
            }

            // Iterate all opponent moves and check is there any that doesn't have check when next player moves
            var opponentMoves = Moves(!isWhiteOffensive, false);
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
            if (_isCheckForOffensivePrecalculated == true)
            {
                return true;
            }
            
            var opponentKing = KingLocation(!isWhiteOffensive);
            if (opponentKing == null) return false; // Test override, don't always have kings on board

            var playerMoves = Moves(isWhiteOffensive, false, false);
            foreach (var singleMove in playerMoves)
            {
                Diagnostics.IncrementCheckCount();
                if (singleMove.NewPos == opponentKing.CurrentPosition)
                {
                    _isCheckForOffensivePrecalculated = true;
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<SingleMove> CollectMoveProperties(IEnumerable<SingleMove> moves)
        {
            foreach (var basicMove in moves)
            {
                yield return CollectMoveProperties(basicMove.PrevPos, basicMove.NewPos);
            }
        }

        public SingleMove CollectMoveProperties(SingleMove move)
        {
            return CollectMoveProperties(move.PrevPos, move.NewPos);
        }

        /// <summary>
        /// Collect before the move is executed to board.
        /// Any changes to board should be done in <see cref="ExecuteMove"/>
        /// </summary>
        /// <returns></returns>
        public SingleMove CollectMoveProperties((int column, int row) from, (int column, int row) to)
        {
            _isCheckForOffensivePrecalculated = null;
            var move = new SingleMove(from, to);
            
            // TODO actual validation

            // Now just check if there is info missing
            var ownPiece = ValueAtDefinitely(from);
            var isWhite = ownPiece.IsWhite;

            // Capture
            var opponentPiece = ValueAt(to);
            if (opponentPiece != null)
            {
                if (opponentPiece.IsWhite != isWhite)
                {
                    move.Capture = true;
                }
                else throw new ArgumentException($"Player with white={isWhite} tried to capture own piece.");
            }

            // Promotion
            if (isWhite && ownPiece.Identity == 'P' && move.NewPos.row == 7)
            {
                move.Promotion = true;
            }
            else if (!isWhite && ownPiece.Identity == 'P' && move.NewPos.row == 0)
            {
                move.Promotion = true;
            }

            // Castling
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

            // Check and checkmate
            var nextBoard = BoardFactory.CreateFromMove(this, move);
            move.Check = nextBoard.IsCheck(isWhite);
            move.CheckMate = nextBoard.IsCheckMate(isWhite, move.Check);
            return move;
        }

        public IEnumerable<SingleMove> FilterOutIllegalMoves(IEnumerable<SingleMove> moves, bool isWhite)
        {
            var legalMoves = Moves(isWhite, false, true);
            foreach (var singleMove in moves)
            {
                var isLegal =
                    legalMoves.FirstOrDefault(m => m.Equals(singleMove));
                if(isLegal != null)
                {
                    // Move is ok
                    yield return singleMove;
                }
                
            }
        }
        
        // Slow
        public IList<(int column, int row)> GetAttackSquares(bool attackerColor)
        {
            // TODO
            return new List<(int column, int row)>();
        }

        public bool CanCastleToLeft(bool white)
        {
            var row = 0;
            if (white)
            {
                if (!Strategic.WhiteLeftCastlingValid) return false;
            }
            else
            {
                if (!Strategic.BlackLeftCastlingValid) return false;
                row = 7;
            }
            
            // Castling pieces are intact
            var rook = ValueAt((0, row));
            var king = ValueAt((4, row));
            if (rook == null || rook.Identity != 'R' || king == null || king.Identity != 'K') return false;
            
            // No other pieces on the way
            if (ValueAt((1, row)) != null) return false;
            if (ValueAt((2, row)) != null) return false;
            if (ValueAt((3, row)) != null) return false;

            // Check that no position is under attack currently.
            var neededSquares = new List<(int, int)> {(1, row), (2, row), (3, row), (4, row)};
            var attackSquares = GetAttackSquares(!white);

            foreach (var position in neededSquares)
            {
                if (attackSquares.Contains(position)) return false;
            }

            return true;
        }

        public bool CanCastleToRight(bool white)
        {
            var row = 0;
            if (white)
            {
                if (!Strategic.WhiteLeftCastlingValid) return false;
            }
            else
            {
                if (!Strategic.BlackLeftCastlingValid) return false;
                row = 7;
            }

            // Castling pieces are intact
            var rook = ValueAt((7, row));
            var king = ValueAt((4, row));
            if (rook == null || rook.Identity != 'R' || king == null || king.Identity != 'K') return false;

            // No other pieces on the way
            if (ValueAt((5, row)) != null) return false;
            if (ValueAt((6, row)) != null) return false;

            // Check that no position is under attack currently.
            var neededSquares = new List<(int, int)> { (4, row), (5, row), (6, row)};
            var attackSquares = GetAttackSquares(!white);

            foreach (var position in neededSquares)
            {
                if (attackSquares.Contains(position)) return false;
            }

            return true;
        }
    }
}
