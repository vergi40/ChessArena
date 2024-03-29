﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;

namespace vergiBlue.Pieces
{
    public class Rook : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Rook(IsWhite, CurrentPosition);

        public Rook(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = PieceBaseStrength.Rook * Direction;
        }

        public Rook(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = PieceBaseStrength.Rook * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            // Start normal relative weighting after halfgame
            if (endGameWeight < 0.5) return PositionStrength;
            return RelativeStrength;
        }

        public override IEnumerable<SingleMove> Moves(IBoard board)
        {
            return RookMoves(board);
        }

        public override PieceBase CreateCopy()
        {
            var piece = new Rook(IsWhite, CurrentPosition);
            return piece;
        }

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return Moves(board);
        }

        public override bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            if (TryCreateRookSliderAttack(board, opponentKing, out sliderAttack))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// If target is found first in direction, true. If some other piece or nothing, false
        /// </summary>
        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            if (TryCreateRookDirectionUInitVector(CurrentPosition, target, out var unitDirection))
            {
                foreach (var next in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, unitDirection))
                {
                    if (target.Equals(next)) return true;
                    if (board.ValueAt(next) != null) return false;
                }
            }

            return false;
        }

        private bool TryCreateRookDirectionDistanceVector((int x, int y) pos1, (int x, int y) pos2, out (int x, int y) dirAndDistance)
        {
            // e.g. piece (4,0), king (2,0). (2,0) - (4,0) = (-2,0) -> two steps left
            dirAndDistance = (pos2.x - pos1.x, pos2.y - pos1.y);
            if (dirAndDistance.x * dirAndDistance.y == 0)
            {
                return true;
            }
            return false;
        }
    }
}
