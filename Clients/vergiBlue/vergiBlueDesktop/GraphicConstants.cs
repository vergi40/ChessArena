using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace vergiBlueDesktop
{
    public static class GraphicConstants
    {
        // Board layout
        public const int BlockSize = 60;
        public static readonly Brush BoardDarkTile = Brushes.DarkGray;
        public static readonly Brush BoardLightTile = Brushes.AntiqueWhite;
        public static readonly Brush BoardBorderColor = Brushes.Black;

        // Visual aids
        public static readonly Brush PlayerMoveColor = Brushes.Chartreuse;
        public static readonly Brush OpponentMoveColor = Brushes.Coral;
        public static readonly Brush CastlingColor = Brushes.BlueViolet;
        public static readonly Brush StartPositionColor = Brushes.Gray;
    }
}
