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
            King newWhite = previousKings.white?.CreateKingCopy(this);
            if (newWhite != null) Pieces[newWhite.CurrentPosition] = newWhite;
            King newBlack = previousKings.black?.CreateKingCopy(this);
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
                Pieces.Remove(move.NewPos);
            }

            var piece = Pieces[move.PrevPos];
            Pieces.Remove(move.PrevPos);
            Pieces.Add(move.NewPos, piece);
            piece.CurrentPosition = move.NewPos;
        }

        private void InitializeFromReference(Board previous)
        {
            foreach (var oldPiece in previous.PieceList)
            {
                var newPiece = oldPiece.CreateCopy(this);
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
                evalScore += 10 * currentSearchDepth;
            }
            else if (evalScore < -StrengthTable.King / 2)
            {
                evalScore -= 10 * currentSearchDepth;
            }

            return evalScore;
        }

        public IEnumerable<SingleMove> Moves(bool forWhite)
        {
            // TODO: Sort moves on end. Priority to moves with capture
            foreach (var piece in PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves())
                {
                    yield return singleMove;
                }
            }
        }

        public void InitializeEmptyBoard()
        {
            // Pawns
            for (int i = 0; i < 8; i++)
            {
                var whitePawn = new Pawn(true, this);
                whitePawn.CurrentPosition = (i, 1);
                AddNew(whitePawn);

                var blackPawn = new Pawn(false, this);
                blackPawn.CurrentPosition = (i, 6);
                AddNew(blackPawn);
            }

            // Rooks
            var rook = new Rook(true, this);
            rook.CurrentPosition = (0,0);
            AddNew(rook);

            rook = new Rook(true, this);
            rook.CurrentPosition = (7, 0);
            AddNew(rook);

            rook = new Rook(false, this);
            rook.CurrentPosition = (0, 7);
            AddNew(rook);

            rook = new Rook(false, this);
            rook.CurrentPosition = (7, 7);
            AddNew(rook);

            var whiteKing = new King(true, this);
            whiteKing.CurrentPosition = "d1".ToTuple();
            AddNew(whiteKing);

            var blackKing = new King(false, this);
            blackKing.CurrentPosition = "d8".ToTuple();
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
                Diagnostics.IncrementEvalCount();
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
                if (singleMove.NewPos == opponentKing.CurrentPosition)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
