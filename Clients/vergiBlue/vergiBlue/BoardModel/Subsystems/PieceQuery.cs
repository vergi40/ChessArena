using System.Collections.Generic;
using System.Linq;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems
{
    public class PieceQuery
    {
        private IBoard _board { get; }

        public PieceQuery(IBoard board)
        {
            _board = board;
        }

        public IEnumerable<IPiece> GetColor(bool forWhite)
        {
            return _board.PieceList.Where(p => p.IsWhite == forWhite);
        }

        public List<IPiece> GetColorList(bool forWhite)
        {
            var list = new List<IPiece>(16);
            foreach (var piece in _board.PieceList)
            {
                if(piece.IsWhite == forWhite) list.Add(piece);
            }

            return list;
        }

        public List<IPiece> AllPawnsList()
        {
            var list = new List<IPiece>(16);
            foreach (var piece in _board.PieceList)
            {
                if (piece.Identity == 'P') list.Add(piece);
            }

            return list;
        }

        public List<IPiece> AllPowerPiecesList()
        {
            var list = new List<IPiece>(16);
            foreach (var piece in _board.PieceList)
            {
                if (piece.Identity != 'P') list.Add(piece);
            }

            return list;
        }
    }
}
