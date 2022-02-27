using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard;
using CommonNetStandard.Interface;
using log4net;
using vergiBlue.Algorithms;
using vergiBlue.BoardModel.SubSystems;
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
        /// Board that was in checkmate was continued
        /// </summary>
        public bool DebugPostCheckMate { get; set; }

        public MoveGenerator MoveGenerator { get; }
        public AttackSquareMapper AttackMapper { get; private set; }

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board()
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            MoveGenerator = new MoveGenerator(this);
            AttackMapper = new AttackSquareMapper();

            Shared = new SharedData();
            Strategic = new StrategicData();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public Board(IBoard other, bool cloneSubSystems)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            MoveGenerator = new MoveGenerator(this);
            AttackMapper = new AttackSquareMapper();

            InitializeFromReference(other);

            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);

            if (cloneSubSystems)
            {
                BoardHash = other.BoardHash;
                if(Shared.UseCachedAttackSquares)
                {
                    AttackMapper = other.AttackMapper.Clone(PieceList);
                }
            }
            else
            {
                InitializeSubSystems();
            }

            UpdateEndGameWeight();
        }

        /// <summary>
        /// Create board setup after move. Clone subsystems
        /// </summary>
        public Board(IBoard other, SingleMove move)
        {
            BoardArray = new PieceBase[8,8];
            PieceList = new List<PieceBase>();
            MoveGenerator = new MoveGenerator(this);
            AttackMapper = new AttackSquareMapper();

            InitializeFromReference(other);
            Shared = other.Shared;
            Strategic = new StrategicData(other.Strategic);

            if (Shared.UseCachedAttackSquares)
            {
                AttackMapper = other.AttackMapper.Clone(PieceList);
            }
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

            AttackMapper = new AttackSquareMapper(this);
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

            InitializeSubSystems();
        }

        public void ExecuteMoveWithValidation(SingleMove move)
        {
            Validator.ValidateMove(this, move);
            ExecuteMove(move);
        }
        
        public void ExecuteMove(SingleMove move)
        {
            BoardHash = Shared.Transpositions.GetNewBoardHash(move, this, BoardHash);
            if(Shared.UseCachedAttackSquares)
            {
                AttackMapper.Update(this, move);
            }

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
                    // TODO continuing after this makes logic really unstable
                    // Ensure validation ends if king is eaten
                    RemovePieces(!isWhite);
                    UpdatePosition(piece, move);
                    DebugPostCheckMate = true;
                    Strategic.EnPassantPossibility = null;
                    return;
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
                piece = move.PromotionType switch
                {
                    PromotionPieceType.Queen => new Queen(piece.IsWhite, move.NewPos),
                    PromotionPieceType.Rook => new Rook(piece.IsWhite, move.NewPos),
                    PromotionPieceType.Bishop => new Bishop(piece.IsWhite, move.NewPos),
                    PromotionPieceType.Knight => new Knight(piece.IsWhite, move.NewPos),
                    _ => throw new ArgumentException($"Unknown promotion: {move.PromotionType}")
                };
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

        /// <summary>
        /// Cast IBoard to Board to use this in testing.
        /// If there is any piece in target square, it's deleted
        /// </summary>
        /// <param name="move"></param>
        public void UpdateBoardArray(SingleMove move)
        {
            var piece = ValueAtDefinitely(move.PrevPos);

            var toBeDeleted = ValueAt(move.NewPos);
            if (toBeDeleted != null)
            {
                RemovePiece(move.NewPos);
            }

            piece.CurrentPosition = move.NewPos;
            BoardArray[move.PrevPos.Item1, move.PrevPos.Item2] = null;
            BoardArray[move.NewPos.Item1, move.NewPos.Item2] = piece;
        }

        public void RemovePiece((int column, int row) position)
        {
            var piece = ValueAt(position);
            if (piece == null) throw new ArgumentException($"Piece in position {position} was null");
            BoardArray[position.column, position.row] = null;

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
        
        private void RemovePieces(bool isWhite)
        {
            var toBeRemoved = PieceList.Where(p => p.IsWhite == isWhite).ToList();
            foreach (var piece in toBeRemoved)
            {
                RemovePiece(piece.CurrentPosition);
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

        public double Evaluate(bool isMaximizing, bool simpleEvaluation,
            int? currentSearchDepth = null)
        {
            return Evaluator.Evaluate(this, isMaximizing, simpleEvaluation, currentSearchDepth);
        }

        public double EvaluateNoMoves(bool isMaximizing, bool simpleEvaluation, int? currentSearchDepth = null)
        {
            // TODO more logic
            return Evaluator.Evaluate(this, isMaximizing, simpleEvaluation, currentSearchDepth);
        }
        
        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        public PieceBase? KingLocation(bool whiteKing)
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
            // TODO double-check that castling moves not needed to validate
            var opponentMoves = MoveGenerator.MovesQuickWithoutCastling(!isWhiteOffensive, false);
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
            if (opponentKing == null)
            {
                DebugPostCheckMate = true;
                return false; // Test override, don't always have kings on board
            }

            if (Shared.UseCachedAttackSquares)
            {
                var isAttacked = AttackMapper.IsPositionAttacked(opponentKing.CurrentPosition, isWhiteOffensive);
                _isCheckForOffensivePrecalculated = true;
                return isAttacked;
            }

            foreach (var attackMove in GetAttackSquares(isWhiteOffensive))
            {
                Diagnostics.IncrementCheckCount();
                if (attackMove == opponentKing.CurrentPosition)
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
                yield return CollectMoveProperties(basicMove);
            }
        }

        public SingleMove CollectMoveProperties(SingleMove initialMove)
        {
            (int column, int row) from = initialMove.PrevPos;
            (int column, int row) to = initialMove.NewPos;

            var ownPiece = ValueAtDefinitely(from);
            var isWhite = ownPiece.IsWhite;


            _isCheckForOffensivePrecalculated = null;
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
            var legalMoves = MoveGenerator.MovesQuick(isWhite, true).ToList();
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
            foreach (var move in MoveGenerator.MovesQuickWithoutCastling(forWhiteAttacker, false))
            {
                yield return move.NewPos;
            }
        }

        /// <summary>
        /// Return as soon as possible
        /// </summary>
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
            // NOTE: heavy on performance, done as last resort
            var neededSquares = new List<(int, int)> {(2, row), (3, row), (4, row)};
            var attackSquares = GetAttackSquares(!white);

            foreach (var target in attackSquares)
            {
                if (neededSquares.Contains(target)) return false;
            }

            return true;
        }

        /// <summary>
        /// Return as soon as possible
        /// </summary>
        public bool CanCastleToRight(bool white)
        {
            var row = 0;
            if (white)
            {
                if (!Strategic.WhiteRightCastlingValid) return false;
            }
            else
            {
                if (!Strategic.BlackRightCastlingValid) return false;
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
            // NOTE: heavy on performance, done as last resort
            var neededSquares = new List<(int, int)> { (4, row), (5, row), (6, row)};
            var attackSquares = GetAttackSquares(!white);

            foreach (var target in attackSquares)
            {
                if (neededSquares.Contains(target)) return false;
            }

            return true;
        }

    }
}
