using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Common;
using vergiBlue.Pieces;

namespace vergiBlue
{
    public class Board
    {
        /// <summary>
        /// Pieces are storaged with (column,row) pair. On algebraic notation [0,0] corresponds to the "a1" notations.
        /// Indexes start from 0
        /// </summary>
        public Dictionary<(int column, int row), PieceBase> Pieces { get; set; } = new Dictionary<(int, int), PieceBase>();

        // Reference
        public Dictionary<(int, int), PieceBase>.ValueCollection PieceList => Pieces.Values;
        public Dictionary<(int, int), PieceBase>.KeyCollection OccupiedCoordinates => Pieces.Keys;

        /// <summary>
        /// Track kings for whole game
        /// </summary>
        public (King white, King black) Kings { get; set; }

        /// <summary>
        /// Start game initialization
        /// </summary>
        public Board(){}

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        /// <param name="previous"></param>
        public Board(Board previous)
        {
            InitializeFromReference(previous);
            InitializeKingsFromReference(previous.Kings);
        }

        /// <summary>
        /// Create board setup after move
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="move"></param>
        public Board(Board previous, SingleMove move)
        {
            InitializeFromReference(previous);
            InitializeKingsFromReference(previous.Kings);
            ExecuteMove(move);
        }

        private void InitializeKingsFromReference((King white, King black) previousKings)
        {
            // Need to ensure kings in board are same as these
            // A bit code smell but works for now
            King newWhite = previousKings.white?.CreateKingCopy();
            if (newWhite != null) Pieces[newWhite.CurrentPosition] = newWhite;
            King newBlack = previousKings.black?.CreateKingCopy();
            if (newBlack != null) Pieces[newBlack.CurrentPosition] = newBlack;
            Kings = (newWhite, newBlack);
        }

        /// <summary>
        /// Apply single move to board.
        /// </summary>
        /// <param name="move"></param>
        public void ExecuteMove(SingleMove move)
        {
            if (move.Capture)
            {
                // Ensure validation ends if king is eaten
                var isWhite = ValueAt(move.PrevPos).IsWhite;
                if (KingLocation(!isWhite).CurrentPosition == move.NewPos)
                {
                    RemovePieces(!isWhite);
                }
                else
                {
                    Pieces.Remove(move.NewPos);
                }
            }

            var piece = Pieces[move.PrevPos];
            piece.CurrentPosition = move.NewPos;
            Pieces.Remove(move.PrevPos);

            if (move.Promotion)
            {
                // Substitute pawn with upgrade
                piece = new Queen(piece.IsWhite, move.NewPos);
            }
            Pieces.Add(move.NewPos, piece);
        }

        private void RemovePieces(bool isWhite)
        {
            var positions = PieceList.Where(p => p.IsWhite == isWhite).Select(p => p.CurrentPosition).ToList();
            foreach (var position in positions)
            {
                Pieces.Remove(position);
            }
        }

        private void InitializeFromReference(Board previous)
        {
            foreach (var oldPiece in previous.PieceList)
            {
                var newPiece = oldPiece.CreateCopy();
                AddNew(newPiece);
            }
        }

        public void InitializeForTesting(Board board)
        {

        }

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        public PieceBase ValueAt((int, int) target)
        {
            if (Pieces.ContainsKey(target)) return Pieces[target];
            return null;
        }

        public void AddNew(PieceBase piece)
        {
            Pieces.Add((piece.CurrentPosition), piece);
        }

        public void AddNew(IEnumerable<PieceBase> pieces)
        {
            foreach (var piece in pieces)
            {
                AddNew(piece);
            }
        }

        private int Direction(bool isWhite)
        {
            if (isWhite) return 1;
            return -1;
        }

        public double Evaluate(bool isMaximizing, int currentSearchDepth)
        {
            // TODO
            Diagnostics.IncrementEvalCount();
            var evalScore = PieceList.Sum(p => p.RelativeStrength);

            // Checkmate (in good or bad) should have more priority the sooner it occurs
            if (evalScore > StrengthTable.King / 2)
            {
                evalScore += 10 * (currentSearchDepth + 1);
            }
            else if (evalScore < -StrengthTable.King / 2)
            {
                evalScore -= 10 * (currentSearchDepth + 1);
            }

            return evalScore;
        }

        public IList<SingleMove> Moves(bool forWhite, bool kingInDanger = false)
        {
            var list = new List<SingleMove>();
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

            // Sort moves with capture first
            return list.OrderByDescending(m => m.Capture).ToList();
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

            Logger.Log("Board initialized.");
        }

        /// <summary>
        /// King location should be known at all times
        /// </summary>
        /// <param name="whiteKing"></param>
        /// <returns></returns>
        private King KingLocation(bool whiteKing)
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
            var opponentMoves = Moves(!isWhiteOffensive);
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

            var playerMoves = Moves(isWhiteOffensive);
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
