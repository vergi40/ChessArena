using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace vergiBlue
{
    class Logic : LogicBase
    {
        private int _index = 2;
        public Move LatestOpponentMove { get; set; }

        public Logic(GameStartInformation startInformation)
        {

        }

        public override PlayerMove CreateMove()
        {
            // TODO testing
            var move = new PlayerMove()
            {
                Move = new Move()
                {
                    StartPosition = $"a{_index--}",
                    EndPosition = $"a{_index}",
                    PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                },
                Diagnostics = "Search depth = 0."
            };

            return move;
        }

        public override void ReceiveMove(Move opponentMove)
        {
            // TODO testing
            LatestOpponentMove = opponentMove;
        }
    }

    public class Board
    {
        /// <summary>
        /// Pieces are storaged with (column,row) pair. On algebraic notation [0,0] corresponds to the "a1" notations.
        /// Indexes start from 0
        /// </summary>
        public Dictionary<(int column, int row), Piece> Pieces { get; set; } = new Dictionary<(int, int), Piece>();

        // Reference
        public Dictionary<(int, int), Piece>.ValueCollection PieceList => Pieces.Values;
        public Dictionary<(int, int), Piece>.KeyCollection OccupiedCoordinates => Pieces.Keys;

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        public Piece ValueAt((int, int) target)
        {
            if (Pieces.ContainsKey(target)) return Pieces[target];
            return null;
        }

        public void AddNew(Piece piece)
        {
            Pieces.Add((piece.CurrentPosition), piece);
        }
    }

    public abstract class Piece
    {
        public bool IsOpponent { get; }
        public bool IsWhite { get; }
        public Board Board { get; }

        public (int column, int row) CurrentPosition { get; set; }

        protected Piece(bool isOpponent, bool isWhite, Board boardReference)
        {
            IsOpponent = isOpponent;
            IsWhite = isWhite;
            Board = boardReference;
        }

        public bool CanMoveTo((int, int) target)
        {
            // TODO
            return false;
        }
        public void MoveTo((int column, int row) target)
        {
            Board.Pieces.Remove(CurrentPosition);
            Board.Pieces.Add(target, this);
        }
    }

    public class Pawn : Piece
    {
        public Pawn(bool isOpponent, bool isWhite, Board boardReference) : base(isOpponent, isWhite, boardReference)
        {
        }
    }

    static class Diagnostics
    {
        private static int EvaluationCount = 0;
        private static List<string> Messages = new List<string>();
        private static readonly object messageLock = new object();

        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementEvalCount()
        {
            Interlocked.Increment(ref EvaluationCount);
        }

        /// <summary>
        /// Thread-safe message operation. Slow
        /// </summary>
        public static void AddMessage(string message)
        {
            // TODO
            lock (messageLock)
            {
                Messages.Add(message);
            }
        }
    }
}
