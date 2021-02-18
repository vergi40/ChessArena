using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueDesktop.Views
{
    public class PieceViewModel : IPieceWithUiControl
    {
        public bool IsWhite { get; set; }
        public Uri SourceUri { get; set; }
        public PieceBase PieceModel { get; set; }

        public MainViewModel Main { get; }

        public PieceViewModel(MainViewModel mainViewModel)
        {
            Main = mainViewModel;}
        
        public void VisualizePossibleTiles()
        {
            var moves = PieceModel.Moves(Main.Board);

            var borderColor = Brushes.Chartreuse;
            if (IsWhite != Main.PlayerIsWhite) borderColor = Brushes.Coral;
            foreach (var singleMove in moves)
            {
                Main.VisualizationTiles.Add(new Position(singleMove.NewPos.row, singleMove.NewPos.column, borderColor));
            }
        }

        public void ClearPossibleTiles()
        {
            Main.VisualizationTiles.Clear();
        }

        public void TurnFinished(Position previousPosition, Position currentPosition)
        {
            var move = new SingleMove((previousPosition.Column, previousPosition.Row),
                (currentPosition.Column, currentPosition.Row));
            Main.TurnFinished(move, false);
        }
    }
}
