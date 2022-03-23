using System;
using System.Collections.Generic;
using System.Linq;
using vergiBlue.BoardModel;

namespace vergiBlue.Pieces
{
    public enum Directions
    {
        N, NE, E, SE, S, SW, W, NW
    }

    public class StaticMoves
    {

        // 1D array used instead of 2D, remember to transform tuples
        public IReadOnlyList<(int column, int row)>[] Knight { get; private set; } = Array.Empty<IReadOnlyList<(int column, int row)>>();
        public IReadOnlyList<(int column, int row)>[] King { get; private set; } = Array.Empty<IReadOnlyList<(int column, int row)>>();
        private Dictionary<int, DirectionMoves> Rook { get; } = new();
        private Dictionary<int, DirectionMoves> Bishop { get; } = new();

        public void Initialize()
        {
            Knight = new IReadOnlyList<(int column, int row)>[64];
            King = new IReadOnlyList<(int column, int row)>[64];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    GenerateKnightRawMovesToPosition((i, j));
                    GenerateKingRawMovesToPosition((i, j));
                    GenerateRookRawMovesToPosition((i, j));
                    GenerateBishopRawMovesToPosition((i,j));
                }
            }
        }

        public IReadOnlyList<(int column, int row)> RookRawMovesToDirection((int column, int row) currentPosition, Directions direction)
        {
            return Rook[currentPosition.To1DimensionArray()].Moves[direction];
        }
        
        public IReadOnlyList<(int column, int row)> BishopRawMovesToDirection((int column, int row) currentPosition, Directions direction)
        {
            return Bishop[currentPosition.To1DimensionArray()].Moves[direction];
        }
        
        private void GenerateKnightRawMovesToPosition((int column, int row) position)
        {
            // Improvement: Skip the SingleMove phase by generating positions here
            var board = new Board(false);
            var knight = new Knight(true, position);

            var moves = knight.MovesValidated(board).Select(m => m.NewPos).ToList();
            Knight[position.To1DimensionArray()] = moves;
        }

        private void GenerateKingRawMovesToPosition((int column, int row) position)
        {
            // Improvement: Skip the SingleMove phase by generating positions here
            var board = new Board(false);
            var king = new King(true, position);

            var moves = king.MovesValidated(board).Select(m => m.NewPos).ToList();
            King[position.To1DimensionArray()] = moves;
        }
        
        private void GenerateRookRawMovesToPosition((int column, int row) position)
        {
            var rook = new Rook(true, position);

            var allMoves = new DirectionMoves();
            allMoves.Moves[Directions.N] = rook.MovesValidatedToDirection((0, 1));
            allMoves.Moves[Directions.E] = rook.MovesValidatedToDirection((1, 0));
            allMoves.Moves[Directions.S] = rook.MovesValidatedToDirection((0, -1));
            allMoves.Moves[Directions.W] = rook.MovesValidatedToDirection((-1, 0));

            Rook.Add(position.To1DimensionArray(), allMoves);
        }

        private void GenerateBishopRawMovesToPosition((int column, int row) position)
        {
            var bishop = new Bishop(true, position);

            var allMoves = new DirectionMoves();
            allMoves.Moves[Directions.NE] = bishop.MovesValidatedToDirection((1, 1));
            allMoves.Moves[Directions.SE] = bishop.MovesValidatedToDirection((1, -1));
            allMoves.Moves[Directions.SW] = bishop.MovesValidatedToDirection((-1, -1));
            allMoves.Moves[Directions.NW] = bishop.MovesValidatedToDirection((-1, 1));

            Bishop.Add(position.To1DimensionArray(), allMoves);
        }
        
        class DirectionMoves
        {
            public Dictionary<Directions, IReadOnlyList<(int column, int row)>> Moves { get; } = new();
        }
    }
}
