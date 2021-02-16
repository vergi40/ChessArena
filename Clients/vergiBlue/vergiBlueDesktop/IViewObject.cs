using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using vergiBlue;

namespace vergiBlueDesktop
{
    // TODO temp copy vGames interfaces
    
    public interface IViewObject
    {
        bool IsWhite { get; }
        
        /// <summary>
        /// Override if needed
        /// </summary>
        bool IsAtStartPosition { get; set; }

        /// <summary>
        /// Current piece row. Left bottom is 0.
        /// </summary>
        int Row { get; }

        /// <summary>
        /// Current piece column. Left bottom is 0.
        /// </summary>
        int Column { get; }

        Position CurrentPosition { get; }
        Position PreviousPosition { get; }

        void UpdatePhysicalLocation(int column, int row, bool initialization);
        void UpdateAbstractLocation(int column, int row);
        void TestIncrRow();
    }

    public interface IPieceWithUiControl : IPiece
    {
        void VisualizePossibleTiles();
        void ClearPossibleTiles();
        void TurnFinished(Position previousPosition, Position currentPosition);
    }

    public interface IPiece
    {
        bool IsWhite { get; }
        
        /// <summary>
        /// Uri to image file
        /// </summary>
        Uri SourceUri { get; set; }
    }

    public class Position : IEquatable<Position>
    {
        public double Strength { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public double UiX => Column * 60;
        public double UiY => Row * 60;
        public Brush BorderColor { get; set; }

        public Position(int row, int column)
        {
            Strength = 0;
            Row = row;
            Column = column;

            BorderColor = Brushes.Chartreuse;
        }

        public bool Equals(Position other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Row == Row && other.Column == Column) return true;
            return false;
        }
    }
}
