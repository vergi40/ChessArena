using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private List<(int column, int row)>[] Knight { get; set; } = Array.Empty<List<(int column, int row)>>();


        public void Initialize()
        {
            Knight = new List<(int column, int row)>[64];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    GenerateKnightRawMovesToPosition((i, j));
                }
            }
        }

        public IReadOnlyList<(int column, int row)> RookRawMoves((int column, int row) currentPosition)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<(int column, int row)> RookRawMovesToDirection((int column, int row) currentPosition, (int x, int y) direction)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<(int column, int row)> BishopRawMoves((int column, int row) currentPosition)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<(int column, int row)> BishopRawMovesToDirection((int column, int row) currentPosition, (int x, int y) direction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Every possible move from position that is inside board.
        /// </summary>
        public IReadOnlyList<(int column, int row)> KnightRawMoves((int column, int row) currentPosition)
        {
            return Knight[currentPosition.To1DimensionArray()];
        }

        private void GenerateKnightRawMovesToPosition((int column, int row) position)
        {
            // Improvement: Skip the SingleMove phase by generating positions here
            var board = new Board(false);
            var knight = new Knight(true, position);

            var moves = knight.MovesValidated(board).Select(m => m.NewPos).ToList();
            Knight[position.To1DimensionArray()] = moves;
        }

        private void GenerateRookRawMovesToPosition((int column, int row) position)
        {
            // TODO
        }

        private void GenerateBishopRawMovesToPosition((int column, int row) position)
        {
            // TODO
        }


        


        //private DirectionMoves[,] Rook { get; set; } = new DirectionMoves[8, 8];
        //private DirectionMoves[,] Bishop { get; set; } = new DirectionMoves[8, 8];

        //class DirectionMoves
        //{
        //    public Dictionary<Directions, List<(int column, int row)>> Moves { get; set; }
        //}

    }
}
