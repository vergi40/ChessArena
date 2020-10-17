﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public abstract class Piece
    {
        public bool IsOpponent { get; }
        public bool IsWhite { get; }

        public abstract double RelativeStrength { get; }

        /// <summary>
        /// Sign of general direction. Can also be used to classify white as positive and black as negative value.
        /// </summary>
        public int Direction
        {
            get
            {
                if (IsWhite) return 1;
                return -1;
            }
        }

        public Board Board { get; }

        public (int column, int row) CurrentPosition { get; set; }

        protected Piece(bool isOpponent, bool isWhite, Board boardReference)
        {
            IsOpponent = isOpponent;
            IsWhite = isWhite;
            Board = boardReference;
        }

        public abstract SingleMove CanMoveTo((int, int) target);

        public void MoveTo((int column, int row) target)
        {
            Board.Pieces.Remove(CurrentPosition);
            Board.Pieces.Add(target, this);
        }

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> Moves();

        public abstract Piece CreateCopy(Board newBoard);
    }

    public class Pawn : Piece
    {
        public override double RelativeStrength { get; }

        public Pawn(bool isOpponent, bool isWhite, Board boardReference) : base(isOpponent, isWhite, boardReference)
        {
            RelativeStrength = StrengthTable.Pawn * Direction;
        }


        /// <summary>
        /// Try if move can be made. Return outcome.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Null if not possible</returns>
        public override SingleMove CanMoveTo((int, int) target)
        {
            if (Logic.IsOutside(target)) return null;
            
            if(Board.ValueAt(target) == null)
            {
                var promotion = target.Item2 == 0 || target.Item2 == 7;
                return new SingleMove(CurrentPosition, target, false, promotion);
            }
            return null;
        }

        private SingleMove CanCapture((int, int) target)
        {
            // Normal
            var diagonal = Board.ValueAt(target);
            if (diagonal != null && diagonal.IsOpponent)
            {
                return new SingleMove(CurrentPosition, diagonal.CurrentPosition, true);
            }

            // En passant - opponent on side
            // Need to check that there is pawn next to and opponent has done double move from start to that pawn last round 
            // TODO
            

            return null;
        }

        /// <summary>
        /// List all allowed
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<SingleMove> Moves()
        {
            var cur = CurrentPosition;

            // Basic
            var move = CanMoveTo((cur.column, cur.row + Direction));
            if (move != null) yield return move;

            // Start possibility
            if(cur.row == 1 || cur.row == 6)
            {
                move = CanMoveTo((cur.column, cur.row + (Direction * 2)));
                if (move != null) yield return move;
            }

            move = CanCapture((cur.column - 1, cur.row + Direction));
            if (move != null) yield return move;
            move = CanCapture((cur.column + 1, cur.row + Direction));
            if (move != null) yield return move;
        }

        public override Piece CreateCopy(Board newBoard)
        {
            var piece = new Pawn(IsOpponent, IsWhite, newBoard);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }
    }
}
